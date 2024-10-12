namespace UbaClone.WebApi.DTOs
{
    public class ChangePaswordDto
    {
        public string Contact { get; set; } = null!;
        public string OldPasword { get; set; } = null!;
        public string NewPasword { get; set;} = null!;
        public string Pin { get; set; } = null!;
    }
}
