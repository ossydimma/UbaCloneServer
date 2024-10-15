namespace UbaClone.WebApi.DTOs
{
    public class SendMoneyDTO
    {
        public int AccountNumber { get; set; }
        public double Balance { get; set; }
        public TransactionDetails? History { get; set; }

    }
}
