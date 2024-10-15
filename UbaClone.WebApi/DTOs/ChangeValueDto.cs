namespace UbaClone.WebApi.DTOs
{
    public class ChangePasswordDto
    {
        public string Contact { get; set; } = null!;
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set;} = null!;
        public string Pin { get; set; } = null!;
    }

    public class ChangePinDto 
    {
        public string Contact { get; set; } = null!;
        public string OldPin { get; set; } = null!;
        public string NewPin { get; set; } = null!;
        public string Password { get; set; } = null!;

    }

    public class VerifyPinDTO
    {
        public string Contact { get; set; } = null!;
        public string Pin { get; set; } = null!;
    }


}
