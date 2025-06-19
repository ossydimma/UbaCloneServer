using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbaClone.WebApi
{
    public class TransactionDetails
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Number { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Narrator { get; set; } = string.Empty;
        public string TypeOfTranscation {  get; set; } = string.Empty;

        // Add this property to link to UbaClone
        public Guid UbaCloneUserId { get; set; }

        // Navigation property for relationship
        public virtual Models.UbaClone UbaCloneUser { get; set; } = null!;

        [Timestamp]
        public byte[]? RowVersion { get; set; }

    }
}
