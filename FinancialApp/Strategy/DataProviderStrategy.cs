using FinancialApp.Factories;
using FinancialApp.Models;
using FinancialApp.Services;
using Microsoft.Extensions.Logging;

namespace FinancialApp.Strategy
{
    public class DataProviderStrategy : IDataProviderStrategy
    {
        private readonly IDataProviderFactory _dataProviderFactory;
        private readonly ILogger<DataProviderStrategy> _logger;

        public DataProviderStrategy(IDataProviderFactory dataProviderFactory, ILogger<DataProviderStrategy> logger)
        {
            _dataProviderFactory = dataProviderFactory;
            _logger = logger;
        }

        public List<FinancialInstrument> GetAvailableInstruments()
        {
            _logger.LogInformation("Getting available instruments from all providers.");
            var tiingoProvider = _dataProviderFactory.GetDataProvider("Forex");
            var binanceProvider = _dataProviderFactory.GetDataProvider("Crypto");

            var tiingoInstruments = tiingoProvider.GetAvailableInstruments();
            var binanceInstruments = binanceProvider.GetAvailableInstruments();

            return tiingoInstruments.Concat(binanceInstruments).ToList();
        }

        public Task<decimal> GetCurrentPrice(string symbol)
        {
            _logger.LogInformation($"Getting current price for symbol: {symbol}");
            var provider = GetProvider(symbol);
            return provider.GetCurrentPrice(symbol);
        }

        public Task SubscribeToPriceUpdates(string symbol)
        {
            _logger.LogInformation($"Subscribing to price updates for symbol: {symbol}");
            var provider = GetProvider(symbol);
            return provider.SubscribeToPriceUpdates(symbol);
        }

        public Task UnsubscribeFromPriceUpdates(string symbol)
        {
            _logger.LogInformation($"Unsubscribing from price updates for symbol: {symbol}");
            var provider = GetProvider(symbol);
            return provider.UnsubscribeFromPriceUpdates(symbol);
        }

        private IDataProvider GetProvider(string symbol)
        {
            if (symbol.ToLower().EndsWith("usd"))
            {
                _logger.LogInformation($"Using Forex provider for symbol: {symbol}");
                return _dataProviderFactory.GetDataProvider("Forex");
            }
            else
            {
                _logger.LogInformation($"Using Crypto provider for symbol: {symbol}");
                return _dataProviderFactory.GetDataProvider("Crypto");
            }
        }
    }
}
