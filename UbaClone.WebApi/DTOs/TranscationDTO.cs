namespace UbaClone.WebApi.DTOs
{
    public class VerifyAccountDTO 
    { 
        public int? Sender {  get; set; }
        public int Receiver {  get; set; }
    }

    public class SendMoneyDTO
    {

        public decimal Amount { get; set; }
        public string SenderPin { get; set; } = string.Empty!;
        public int ReceiversAccountNumber { get; set; }
        public int SenderAccountNumber { get; set; }
        public string Narrator { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty!;
        public string Time { get; set; } = string.Empty!;

    }
}
