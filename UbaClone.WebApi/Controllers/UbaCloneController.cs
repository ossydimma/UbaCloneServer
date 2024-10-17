using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Channels;
using UbaClone.WebApi.Data;
using UbaClone.WebApi.DTOs;
using UbaClone.WebApi.Models;
using UbaClone.WebApi.Repositories;

namespace UbaClone.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UbaCloneController(IUsersRepository repo, IConfiguration configuration) : ControllerBase
    {
        private readonly IUsersRepository _repo = repo;
        private readonly IConfiguration _config = configuration;

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Models.UbaClone>) )]
        public async Task<IEnumerable<Models.UbaClone>> GetAll()
        {
            //var clones = await _db.ubaClones.ToListAsync();
            //return Ok(clones);
            return await  _repo.RetrieveAllAsync();
        }

        [HttpGet("{accountNumber}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUsersName(int accountNumber)
        {
            Models.UbaClone? user = await _repo.GetUserByAccountNo(accountNumber);
            if (user == null) return NotFound("You have entered an invalid beneficiary account number, please enter the correct and try again");

            return Ok(user.FullName);
        }

        [HttpPost("Verify-Account")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBeneficiary(VerifyAccountDTO model)
        {

            if (model.Receiver == model.Sender) return BadRequest("You can't make transfer to your account");

            Models.UbaClone? user = await _repo.GetUserByAccountNo(model.Receiver);
            if (user == null) return NotFound("You have entered an invalid beneficiary account number, please enter the correct and try again");

            return Ok(user.FullName);
        }

        [HttpPost("Sign-in")]
        [ProducesResponseType(200, Type = typeof(Models.UbaClone))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto registerUser)
        {
            if (registerUser is null) return BadRequest();

            Models.UbaClone? existing = await _repo.GetUserByContactAsync(registerUser.Contact);
            if (existing is not null)
                return BadRequest($"User already exist,\nCan't register another user with this same contact:{registerUser.Contact}");

            int maxAccountNo = await _repo.GetMaxAccountNo() ?? 20000000;

            Models.UbaClone user = new()
            {
                FullName = registerUser.FullName,
                Contact = registerUser.Contact,
                AccountNumber = maxAccountNo + 1,
                Balance = 20000,
                TransactionHistory = []
            };

            Hasher.CreateValueHash(registerUser.Password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            Hasher.CreateValueHash(registerUser.Pin, out byte[] pinHash, out byte[] pinSalt);
            user.PinHash = pinHash;
            user.PinSalt = pinSalt;



            Models.UbaClone? addedUser = await _repo.CreateUserAsync(user);

            if (addedUser == null) return BadRequest("Repository failed to create user");

            return Ok("Registered Successfuly");

        }

        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Models.UbaClone? user = await _repo.GetUserByContactAsync(loginDto.Contact);
            if (user is null) 
                return Unauthorized("You don't have an account with us.");

            if (!_repo.VerifyPasswordAsync(user, loginDto.Password))
                return Unauthorized("Dear customer, You've entered an invalid password, Did you forget your password?");

            

            var jwtSetting = _config.GetSection("JwtSettings");

            var key = Encoding.UTF8.GetBytes(jwtSetting["Secret"]!);
            var issuer = jwtSetting["Issuer"];
            var audience = jwtSetting["Audience"]; 
            var expirationMinutes = int.Parse(jwtSetting["ExpirationMinutes"]!);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("FullName" , user.FullName),
                    new Claim ("Contact", user.Contact),
                    new Claim ("Balance", user.Balance.ToString()),
                    new Claim ("AccountNumber", user.AccountNumber.ToString()),
                    new Claim("History", JsonConvert.SerializeObject(user.TransactionHistory))
                    
                }), 
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)      
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(tokenHandler.WriteToken(token));
        }

        [HttpPost("Transfer-Money")]
        [ProducesResponseType(400)]
        public async Task<IActionResult> TransferMoney(SendMoneyDTO model)
        {
            if (!ModelState.IsValid) return BadRequest("Sent Data does not match request data");

            Models.UbaClone? sender = await _repo.GetUserByAccountNo(model.SenderAccountNumber);
            if (sender == null) return BadRequest("Transcation failed.");

            Models.UbaClone? receiver = await _repo.GetUserByAccountNo(model.ReceiversAccountNumber);
            if (receiver == null) return BadRequest("Transaction failed, Verify beneficiary account number");

            if ( ! _repo.VerifyPinAsync(sender, model.SenderPin) )
                return BadRequest("Entered an Invalid PIN");

            if (sender.Balance < model.Amount) return BadRequest("Insufficient fund");

            sender.Balance -= model.Amount;
            receiver.Balance += model.Amount;
            TransactionDetails receiverHistory = new()
            {
                Amount = model.Amount,
                Name = sender.FullName,
                Narrator = model.Narrator,  
                Number = sender.AccountNumber,
                Date = model.Date,
                Time = model.Time,
                TypeOfTranscation = "Credit",
            };
            TransactionDetails senderHistory = new()
            {
                Amount = model.Amount,
                Name = receiver.FullName,
                Narrator = model.Narrator,
                Number = receiver.AccountNumber,
                Date = model.Date,
                Time = model.Time,
                TypeOfTranscation = "Debit",
            };


            sender.TransactionHistory.Add(senderHistory);
            receiver.TransactionHistory.Add(receiverHistory);

            
            bool savedSender = await _repo.SaveAsync(sender);
            bool savedReceiver = await _repo.SaveAsync(receiver);

            if (!savedSender && !savedReceiver) 
                return BadRequest("Transaction failed");
            var jwtSetting = _config.GetSection("JwtSettings");

            var key = Encoding.UTF8.GetBytes(jwtSetting["Secret"]!);
            var issuer = jwtSetting["Issuer"];
            var audience = jwtSetting["Audience"];
            var expirationMinutes = int.Parse(jwtSetting["ExpirationMinutes"]!);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("FullName" , sender.FullName),
                    new Claim ("Contact", sender.Contact),
                    new Claim ("Balance", sender.Balance.ToString()),
                    new Claim ("AccountNumber", sender.AccountNumber.ToString()),
                    new Claim("History", JsonConvert.SerializeObject(sender.TransactionHistory))

                }),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(tokenHandler.WriteToken(token));

            //return Ok($"You have successfully transferred NGN{model.Amount} to {receiver.FullName} Account Number: {receiver.AccountNumber}");


        }

        [HttpPut("change-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Models.UbaClone? user = await _repo.GetUserByContactAsync(model.Contact);
            if (user is null)
                return NotFound ($" {model.Contact} Not found.");
            
            if ( !_repo.VerifyPasswordAsync(user, model.OldPassword) )
                 return BadRequest("old password is incorrect.");

            if (_repo.VerifyPasswordAsync(user, model.NewPassword))
                return BadRequest("Old and New password cannot be the same");

            if ( !_repo.VerifyPinAsync(user, model.Pin) )
                return BadRequest("invalid Pin.");

            await _repo.ChangePasswordAsync(user, model.NewPassword);

            return Ok("Password changed successfully.");
        }

        [HttpPut("Forgotten-Password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ForgottenPassword(ForgottenPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Models.UbaClone? user = await  _repo.GetUserByContactAsync(model.Contact);
            if (user is null)
                return NotFound($"{model.Contact} is invalid. ");

            if (!_repo.VerifyPinAsync(user, model.Pin))
                return BadRequest("Invalid PIN");

            await _repo.ChangePasswordAsync(user, model.NewPassword);

            return Ok("Password Changed Successfully");

        }

        [HttpPut("change-PIN")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangePin(ChangePinDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Models.UbaClone? user = await _repo.GetUserByContactAsync(model.Contact);
            if (user is null)
                return NotFound($" User:{model.Contact} Not found. ");

            if (!_repo.VerifyPinAsync(user, model.OldPin))
                return BadRequest("old Pin is invalid.");

            if (!_repo.VerifyPasswordAsync(user, model.Password))
                return BadRequest("Entered an incorrect Password.");

            await _repo.ChangePinAsync(user, model.NewPin);

            return Ok("PIN changed Successfully");
        }

        [HttpPut("Forgotten-PIN")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ForgottenPin(ForgottenPinDTO model)
        {
            // This method is securied enough just for learning seek

            if (!ModelState.IsValid) return BadRequest(ModelState);

            Models.UbaClone? user = await _repo.GetUserByContactAsync(model.Contact);
            if (user is null)
                return NotFound($" User:{model.Contact} not found. ");

            if (!_repo.VerifyPasswordAsync(user, model.Password))
                return BadRequest("Incorrect Password. ");

            await _repo.ChangePinAsync(user, model.NewPin);

            return Ok("PIN changed Sucessfully");
        }

        [HttpDelete("{contact}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(string contact)
        {
            Models.UbaClone? user = await _repo.GetUserByContactAsync(contact);
            if (user is null)
                return NotFound("User not found. ");

            bool? deleted = await _repo.DeleteUserAsync(user.UserId);

            if (deleted.HasValue && deleted.Value) return new NoContentResult();

            return BadRequest($"User {user.Contact} was found but failed to delete");
        }

    }
   


}
