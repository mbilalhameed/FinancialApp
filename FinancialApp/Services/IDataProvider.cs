using FinancialApp.Models;

namespace FinancialApp.Services
{
    public interface IDataProvider
    {
        List<FinancialInstrument> GetAvailableInstruments();
        Task<decimal> GetCurrentPrice(string symbol);
        Task SubscribeToPriceUpdates(string symbol);
        Task UnsubscribeFromPriceUpdates(string symbol);
    }
}
