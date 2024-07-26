using FinancialApp.Services;
using Microsoft.Extensions.Logging;

namespace FinancialApp.Factories
{
    public class DataProviderFactory : IDataProviderFactory
    {
        private readonly Dictionary<string, IDataProvider> _dataProviders;
        private readonly ILogger<DataProviderFactory> _logger;

        public DataProviderFactory(Dictionary<string, IDataProvider> dataProviders, ILogger<DataProviderFactory> logger)
        {
            _dataProviders = dataProviders ?? throw new ArgumentNullException(nameof(dataProviders));
            _logger = logger;
        }

        public IDataProvider GetDataProvider(string providerName)
        {
            _logger.LogInformation($"Requesting data provider for type: {providerName}");

            if (_dataProviders.TryGetValue(providerName.ToLower(), out var provider))
            {
                _logger.LogInformation($"Data provider for type {providerName} found.");
                return provider;
            }

            _logger.LogWarning($"Data provider for type {providerName} not found.");
            throw new ArgumentException($"Data provider '{providerName}' not found.");
        }
    }
}
