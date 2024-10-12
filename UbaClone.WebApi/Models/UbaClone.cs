namespace UbaClone.WebApi.Models
{
    public class UbaClone
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = null!;
        public byte[] PasswordSalt { get; set; } = null!;
        public byte[] PinHash { get; set; } = null!;
        public byte[] PinSalt { get; set; } = null!;
        public string Contact { get; set; } = string.Empty;
        public int AccountNumber { get; set; }
        public double Balance { get; set; }

        public TransactionDetails[]? History;
    }
}
