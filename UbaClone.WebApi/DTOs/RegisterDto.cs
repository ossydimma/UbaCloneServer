namespace UbaClone.WebApi.DTOs;

public class RegisterDto
{
    public string FullName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Pin { get; set; } = null!;
    public string Contact { get; set; } = null!;
    //public string AccountType { get; set; } = "Current Account";
}

public class LoginDto 
{
    public string Contact {  set; get; } = null!;
    public string Password { set; get; } = null!;
}

