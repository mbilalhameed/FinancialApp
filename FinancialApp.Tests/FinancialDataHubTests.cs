using Moq;
using Xunit;
using FinancialApp.Hubs;
using FinancialApp.Models;
using Microsoft.AspNetCore.SignalR;

namespace FinancialApp.Tests.Hubs
{
    public class FinancialDataHubTests
    {
        private readonly Mock<IFinancialDataObserver> _financialDataObserverMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly FinancialDataHub _hub;

        public FinancialDataHubTests()
        {
            _financialDataObserverMock = new Mock<IFinancialDataObserver>();
            _clientProxyMock = new Mock<IClientProxy>();

            var mockClients = new Mock<IHubCallerClients>();
            mockClients.Setup(x => x.All).Returns(_clientProxyMock.Object);

            _hub = new FinancialDataHub(_financialDataObserverMock.Object)
            {
                Clients = mockClients.Object
            };
        }

        [Fact]
        public async Task OnConnectedAsync_SubscribesToObserver()
        {
            // Arrange
            var context = new Mock<HubCallerContext>();
            _hub.Context = context.Object;

            // Act
            await _hub.OnConnectedAsync();

            // Assert
            _financialDataObserverMock.Verify(x => x.Subscribe(_hub), Times.Once);
        }

        [Fact]
        public async Task OnDisconnectedAsync_UnsubscribesFromObserver()
        {
            // Arrange
            var context = new Mock<HubCallerContext>();
            _hub.Context = context.Object;

            // Act
            await _hub.OnDisconnectedAsync(null);

            // Assert
            _financialDataObserverMock.Verify(x => x.Unsubscribe(_hub), Times.Once);
        }

        [Fact]
        public void OnNext_SendsPriceUpdateToClients()
        {
            // Arrange
            var priceUpdate = new PriceUpdate { Symbol = "btcusdt", Price = 50000m };

            // Act
            _hub.OnNext(priceUpdate);

            // Assert
            _clientProxyMock.Verify(
                x => x.SendCoreAsync(
                    "ReceivePriceUpdate",
                    new[] { priceUpdate },
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }
    }
}
