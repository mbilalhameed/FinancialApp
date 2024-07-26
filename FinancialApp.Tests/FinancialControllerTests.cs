using FinancialApp.Controllers;
using FinancialApp.Models;
using FinancialApp.Strategy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinancialApp.Tests.Controllers
{
    public class FinancialControllerTests
    {
        private readonly Mock<IDataProviderStrategy> _dataProviderStrategyMock;
        private readonly Mock<ILogger<FinancialController>> _loggerMock;
        private readonly FinancialController _controller;

        public FinancialControllerTests()
        {
            _dataProviderStrategyMock = new Mock<IDataProviderStrategy>();
            _loggerMock = new Mock<ILogger<FinancialController>>();
            _controller = new FinancialController(_dataProviderStrategyMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetAvailableInstruments_ReturnsList()
        {
            // Arrange
            var instruments = new List<FinancialInstrument>
            {
                new FinancialInstrument { Symbol = "btcusdt", Name = "Bitcoin", Type = "Crypto" }
            };
            _dataProviderStrategyMock.Setup(x => x.GetAvailableInstruments()).Returns(instruments);

            // Act
            var result = _controller.GetAvailableInstruments() as ActionResult<IEnumerable<FinancialInstrument>>;

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FinancialInstrument>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetCurrentPrice_ReturnsPrice()
        {
            // Arrange
            var symbol = "btcusdt";
            var price = 50000m;
            _dataProviderStrategyMock.Setup(x => x.GetCurrentPrice(symbol)).ReturnsAsync(price);

            // Act
            var result = await _controller.GetCurrentPrice(symbol) as ActionResult<double>;

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(price, returnValue);
        }

        [Fact]
        public async Task SubscribeToPriceUpdates_ValidSymbol_ReturnsOk()
        {
            // Arrange
            var symbol = "btcusdt";
            _dataProviderStrategyMock.Setup(x => x.SubscribeToPriceUpdates(symbol)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SubscribeToPriceUpdates(symbol);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UnsubscribeFromPriceUpdates_ValidSymbol_ReturnsOk()
        {
            // Arrange
            var symbol = "btcusdt";
            _dataProviderStrategyMock.Setup(x => x.UnsubscribeFromPriceUpdates(symbol)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UnsubscribeFromPriceUpdates(symbol);

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}
