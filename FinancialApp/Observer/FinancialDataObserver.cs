using FinancialApp.Models;

namespace FinancialApp.Observers
{
    public class FinancialDataObserver : IFinancialDataObserver
    {
        private readonly List<IObserver<PriceUpdate>> _observers;

        public FinancialDataObserver()
        {
            _observers = new List<IObserver<PriceUpdate>>();
        }

        public void Subscribe(IObserver<PriceUpdate> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Unsubscribe(IObserver<PriceUpdate> observer)
        {
            if (_observers.Contains(observer))
                _observers.Remove(observer);
        }

        public void Notify(PriceUpdate update)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(update);
            }
        }
    }
}
