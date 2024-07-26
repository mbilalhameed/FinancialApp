using FinancialApp.Models;

public interface IFinancialDataObserver
{
    void Subscribe(IObserver<PriceUpdate> observer);
    void Unsubscribe(IObserver<PriceUpdate> observer);
    void Notify(PriceUpdate update);
}
