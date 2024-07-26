using FinancialApp.Services;

namespace FinancialApp.Factories
{
    public interface IDataProviderFactory
    {
        IDataProvider GetDataProvider(string providerName);
    }
}
