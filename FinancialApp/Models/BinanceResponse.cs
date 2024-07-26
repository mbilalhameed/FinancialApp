namespace FinancialApp.Models
{
    public class BinanceResponse
    {
        public string symbol { get; set; }
        public decimal price { get; set; }
        public long time { get; set; }
    }
}
