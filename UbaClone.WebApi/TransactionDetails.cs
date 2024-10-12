namespace UbaClone.WebApi
{
    public class TransactionDetails
    {

        public string Name { get; set; } = null!;
        public int Number { get; set; } 
        public int Amount { get; set; }
        public int Narrator { get; set; }
        public DateTime CurrentDateTime { get; set; }

    }
}
