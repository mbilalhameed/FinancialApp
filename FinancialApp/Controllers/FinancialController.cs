using Microsoft.AspNetCore.Mvc;
using FinancialApp.Models;
using FinancialApp.Strategy;

namespace FinancialApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialController : ControllerBase
    {
        private readonly IDataProviderStrategy _dataProviderStrategy;
        private readonly ILogger<FinancialController> _logger;

        public FinancialController(IDataProviderStrategy dataProviderStrategy, ILogger<FinancialController> logger)
        {
            _dataProviderStrategy = dataProviderStrategy;
            _logger = logger;
        }

        [HttpGet("instruments")]
        public ActionResult<IEnumerable<FinancialInstrument>> GetAvailableInstruments()
        {
            _logger.LogInformation("Fetching available instruments.");
            return Ok(_dataProviderStrategy.GetAvailableInstruments());
        }

        [HttpGet("price/{symbol}")]
        public async Task<ActionResult<double>> GetCurrentPrice(string symbol)
        {
            _logger.LogInformation($"Fetching current price for symbol: {symbol}");
            return Ok(await _dataProviderStrategy.GetCurrentPrice(symbol));
        }

        [HttpPost("subscribe/{symbol}")]
        public async Task<IActionResult> SubscribeToPriceUpdates(string symbol)
        {
            _logger.LogInformation($"Subscribing to price updates for symbol: {symbol}");
            await _dataProviderStrategy.SubscribeToPriceUpdates(symbol);
            return Ok();
        }

        [HttpPost("unsubscribe/{symbol}")]
        public async Task<IActionResult> UnsubscribeFromPriceUpdates(string symbol)
        {
            _logger.LogInformation($"Unsubscribing from price updates for symbol: {symbol}");
            await _dataProviderStrategy.UnsubscribeFromPriceUpdates(symbol);
            return Ok();
        }
    }
}
