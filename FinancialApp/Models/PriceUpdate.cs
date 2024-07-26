﻿namespace FinancialApp.Models
{
    public class PriceUpdate
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
