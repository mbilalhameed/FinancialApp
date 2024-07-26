using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FinancialApp.Models;
using FinancialApp.Observers;

namespace FinancialApp.Services
{
    public class TiingoService : IDataProvider
    {
        private readonly HttpClient _httpClient;
        private readonly FinancialDataObserver _financialDataObserver;
        private readonly ILogger<TiingoService> _logger;
        private ClientWebSocket _webSocket;
        private readonly string ApiKey;
        private readonly string _wsUrl = "wss://api.tiingo.com/fx";
        private readonly string _rstUrl = "https://api.tiingo.com/tiingo/fx";

        public TiingoService(HttpClient httpClient, FinancialDataObserver financialDataObserver, ILogger<TiingoService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _financialDataObserver = financialDataObserver;
            _logger = logger;
            ApiKey = configuration["Tiingo:ApiKey"];
        }

        public async Task<decimal> GetCurrentPrice(string symbol)
        {
            var response = await _httpClient.GetAsync($"{_rstUrl}/top?tickers={symbol}&token={ApiKey}");
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<List<TiingoResponse>>();
            return data[0].askPrice; // Example field, adjust according to actual API response
        }

        private async Task SendSubscribeMessage(string symbol)
        {
            var subscribeMessage = new
            {
                eventName = "subscribe",
                authorization = ApiKey,
                eventData = new { thresholdLevel = 5, tickers = new[] { $"{symbol}" } }
            };
            var messageJson = JsonSerializer.Serialize(subscribeMessage);
            var messageBuffer = Encoding.UTF8.GetBytes(messageJson);
            await _webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            _logger.LogInformation($"Subscribed to {symbol} ticker updates.");
        }

        private async void StartReceiving()
        {
            var buffer = new byte[1024 * 4];
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogInformation($"Message received: {messageJson}");
                    var priceUpdate = JsonSerializer.Deserialize<PriceUpdate>(messageJson);
                    _financialDataObserver.Notify(priceUpdate);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    _logger.LogInformation("WebSocket closed.");
                }
            }
        }

        public List<FinancialInstrument> GetAvailableInstruments()
        {
            return new List<FinancialInstrument>()
            {
                new FinancialInstrument() {Symbol="audusd", Name="AUD/USD", Type="Forex"},
                new FinancialInstrument() {Symbol="eurusd", Name="EUR/USD", Type="Forex"},

            };
        }

        public async Task SubscribeToPriceUpdates(string symbol)
        {
            if (_webSocket == null)
            {
                try
                {
                    _webSocket = new ClientWebSocket();

                    _logger.LogInformation($"Connecting to Tiingo WebSocket at {_wsUrl}");
                    await _webSocket.ConnectAsync(new Uri(_wsUrl), CancellationToken.None);
                    _logger.LogInformation("Connected to Tiingo WebSocket");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error connecting to Tiingo WebSocket: {ex.Message}");
                }

                await SendSubscribeMessage(symbol);
                StartReceiving();
            }
            else
            {
                await SendSubscribeMessage(symbol);
                StartReceiving();
            }
        }

        public async Task UnsubscribeFromPriceUpdates(string symbol)
        {
            if (_webSocket != null)
            {
                var unsubscribeMessage = new
                {
                    eventName = "unsubscribe",
                    authorization = ApiKey,
                    eventData = new { tickers = new[] { symbol } }
                };
                var messageJson = JsonSerializer.Serialize(unsubscribeMessage);
                var messageBuffer = Encoding.UTF8.GetBytes(messageJson);
                await _webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.LogInformation($"Unsubscribed from {symbol} ticker updates.");
            }
        }
    }
}
