using System.ComponentModel.DataAnnotations.Schema;

namespace UbaClone.WebApi.Models
{
    public class UbaClone
    {
        //change id type to guild
        public Guid Id { get; set; } = new Guid();
        public string FullName { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = null!;
        public byte[] PasswordSalt { get; set; } = null!;
        public byte[] PinHash { get; set; } = null!;
        public byte[] PinSalt { get; set; } = null!;
        public string Contact { get; set; } = string.Empty;
        public int AccountNumber { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        public List<TransactionDetails> TransactionHistory = [];
    }
}
