using System.ComponentModel.DataAnnotations.Schema;

namespace UbaClone.WebApi.DTOs
{
    public class HistoryDTO
    {
        public string Name { get; set; } = null!;
        public int Number { get; set; }
        public decimal Amount { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Narrator { get; set; } = string.Empty;
        public string TypeOfTranscation { get; set; } = string.Empty;
    }
}
