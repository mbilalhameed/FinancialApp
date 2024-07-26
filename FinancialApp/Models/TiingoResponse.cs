namespace FinancialApp.Models
{
    public class TiingoResponse
    {
        public string ticker { get; set; }
        public DateTimeOffset quoteTimestamp { get; set; }
        public decimal bidPrice { get; set; }
        public decimal bidSize { get; set; }
        public decimal askPrice { get; set; }
        public decimal askSize { get; set; }
        public decimal midPrice { get; set; }
    }
}
