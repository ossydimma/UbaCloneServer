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
        [HttpPost("Sign-in")]
        [ProducesResponseType(200, Type = typeof(Models.UbaClone))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto registerUser)
        {
            if (registerUser is null)
            {
                return BadRequest();
            }

            int maxAccountNo = await _repo.GetMaxAccountNo() ?? 200000000;

            Models.UbaClone user = new()
            {
                FullName = registerUser.FullName,
                Contact = registerUser.Contact,
                AccountNumber = maxAccountNo + 1,
                Balance = 0,
                History = []
            };

            Hasher.CreateValueHash(registerUser.Password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            Hasher.CreateValueHash(registerUser.Pin, out byte[] pinHash, out byte[] pinSalt);
            user.PinHash = pinHash;
            user.PinSalt = pinSalt;



            Models.UbaClone? addedUser = await _repo.CreateUserAsync(user);

            if (addedUser == null) return BadRequest("Repository failed to create user");

            return Ok(registerUser);

        }

        [HttpPut("{contact}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(string contact, [FromBody] Models.UbaClone user)
        {
            if (user is null || user.Contact != contact) return BadRequest();

            Models.UbaClone? existing = await _repo.GetUserByContactAsync(contact);
            if (existing == null) return NotFound();

            await _repo.UpdateUserAsync(user);
            return new NoContentResult();
        }

        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest();

            Models.UbaClone? user = await _repo.GetUserByContactAsync(loginDto.Contact);
            if (user is null) 
                return Unauthorized("Invalid contact or passwod");

            bool isValidPassword = await _repo.VerifyPasswordAsync(user.Contact, loginDto.Password);
            if (!isValidPassword)
                return Unauthorized("Invalid contact or passsword");

            

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
                    new Claim(ClaimTypes.Name , user.FullName),
                    new Claim (ClaimTypes.MobilePhone, user.Contact),
                    new Claim ("balance", user.Balance.ToString()),
                    new Claim ("AccountNumber", user.AccountNumber.ToString()),
                    new Claim("History", JsonConvert.SerializeObject(user.History))
                }), 
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)      
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(tokenHandler.WriteToken(token));
        }

        [HttpPut("change-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePaswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool validPassword = await _repo.VerifyPasswordAsync(model.Contact, model.OldPasword);

            if (!validPassword) return BadRequest("old password is incorrect.");

            bool validPin = await _repo.VerifyPinAsync(model.Contact, model.Pin);
            if (!validPin) return BadRequest("invalid Pin.");

            await _repo.ChangePasswordAsync(model.Contact, model.NewPasword);
            return Ok("Password changed successfully.");
        }


        //[HttpGet("{id}", Name = nameof(GetUser) )]
        //[ProducesResponseType(200, Type = typeof(Models.UbaClone))]
        //[ProducesResponseType(404)]
        //public async Task<IActionResult> GetUser(int id)
        //{

        //    Models.UbaClone? user = await _repo.RetrieveAsync(id);
        //    if (user == null) return NotFound("user Not found");

        //    return Ok(user);
        //}



        //[HttpPut]
        //public async Task<IActionResult> ChangePassword(int id,ChangePaswordDto changedValue)
        //{
        //    if (user is null || user.Id != id) return BadRequest();

        //    Models.UbaClone? existing = await _repo.RetrieveAsync(id);
        //    if (existing == null) return NotFound();

        //    //thinking on add getUserByContact at the interface

        //}
        //[HttpDelete("{id}")]
        //[ProducesResponseType(204)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        //public async Task<IActionResult> DeleteUser(string contact)
        //{
        //    Models.UbaClone? user = await _repo.GetUserByContactAsync(contact);
        //    if (user == null) return NotFound();

        //    bool? deleted = await _repo.DeleteUserAsync(id);

        //    if (deleted.HasValue && deleted.Value) return new NoContentResult();

        //    return BadRequest($"User {id} was found but failed to delete");
        //}

    }
}
