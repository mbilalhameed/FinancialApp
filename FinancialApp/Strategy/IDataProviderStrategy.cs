using FinancialApp.Models;

namespace FinancialApp.Strategy
{
    public interface IDataProviderStrategy
    {
        List<FinancialInstrument> GetAvailableInstruments();
        Task<decimal> GetCurrentPrice(string symbol);
        Task SubscribeToPriceUpdates(string symbol);
        Task UnsubscribeFromPriceUpdates(string symbol);
    }
}
