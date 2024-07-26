using Microsoft.AspNetCore.SignalR;
using FinancialApp.Models;
using FinancialApp.Observers;

namespace FinancialApp.Hubs
{
    public class FinancialDataHub : Hub, IObserver<PriceUpdate>
    {
        private readonly IFinancialDataObserver _financialDataObserver;

        public FinancialDataHub(IFinancialDataObserver financialDataObserver)
        {
            _financialDataObserver = financialDataObserver;
        }

        public override async Task OnConnectedAsync()
        {
            _financialDataObserver.Subscribe(this);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _financialDataObserver.Unsubscribe(this);
            await base.OnDisconnectedAsync(exception);
        }

        public void OnNext(PriceUpdate value)
        {
            Clients.All.SendAsync("ReceivePriceUpdate", value);
        }

        public void OnError(Exception error)
        {
            // Handle errors here
        }

        public void OnCompleted()
        {
            // Handle completion here
        }
    }
}
