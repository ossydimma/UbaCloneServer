namespace UbaClone.WebApi.DTOs
{
    public class ForgottenPasswordDTO
    {
        public string Contact { get; set; } = null!;
        public string NewPassword { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
    }

    public class ForgottenPinDTO 
    {
        public string Contact { get; set; } = null!;
        public string Password { get; set; } = string.Empty;
        public string NewPin { get; set; } = string.Empty;

    }

}
