using System.Net.WebSockets;
using System.Text;
using FinancialApp.Models;
using FinancialApp.Observers;
using Newtonsoft.Json;

namespace FinancialApp.Services
{
    public class BinanceService : IDataProvider
    {
        private readonly HttpClient _httpClient;
        private readonly FinancialDataObserver _financialDataObserver;
        private ClientWebSocket _webSocket;
        private readonly ILogger<BinanceService> _logger;
        private readonly  string _wsUrl = "wss://stream.binance.com:9443/ws";
        private readonly  string _rstUrl = "https://testnet.binancefuture.com/fapi/v2/ticker";

        public BinanceService(HttpClient httpClient, FinancialDataObserver financialDataObserver, ILogger<BinanceService> logger)
        {
            _httpClient = httpClient;
            _financialDataObserver = financialDataObserver;
            _logger = logger;
        }

        public List<FinancialInstrument> GetAvailableInstruments()
        {
            return new List<FinancialInstrument>()
            {
                new FinancialInstrument() {Symbol="btcusdt", Name="Bitcoin", Type="Crypto"},
                new FinancialInstrument() {Symbol="ethusdt", Name="Ethereum", Type="Crypto"},
                new FinancialInstrument() {Symbol="solusdt", Name="Solana", Type="Crypto"},

            };
        }

        public async Task<decimal> GetCurrentPrice(string symbol)
        {
            var response = await _httpClient.GetAsync($"{_rstUrl}/price?symbol={symbol}");
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<BinanceResponse>();
            return data.price;
        }

        public async Task SubscribeToPriceUpdates(string symbol)
        {
            if (_webSocket == null)
            {
                try
                {
                    _webSocket = new ClientWebSocket();
                    _logger.LogInformation($"Connecting to Binance WebSocket at {_wsUrl}");
                    await _webSocket.ConnectAsync(new Uri(_wsUrl), CancellationToken.None);
                    _logger.LogInformation("Connected to Binance WebSocket");
                }
                catch (Exception ex)
                {

                    _logger.LogError($"Error connecting to Binance WebSocket: {ex.Message}");
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
                    method = "UNSUBSCRIBE",
                    @params = new[] { $"{symbol.ToLower()}@aggTrade" },
                    id = 1
                };
                var messageJson = JsonConvert.SerializeObject(unsubscribeMessage);
                var messageBuffer = Encoding.UTF8.GetBytes(messageJson);
                await _webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.LogInformation($"Unsubscribed from {symbol} ticker updates.");
            }
        }

        private async Task SendSubscribeMessage(string symbol)
        {
            var subscribeMessage = new
            {
                method = "SUBSCRIBE",
                @params = new[] { $"{symbol.ToLower()}@aggTrade" },
                id = 1
            };
            var messageJson = JsonConvert.SerializeObject(subscribeMessage);
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
                    var priceUpdate = JsonConvert.DeserializeObject<PriceUpdate>(messageJson);
                    _financialDataObserver.Notify(priceUpdate);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    _logger.LogInformation("WebSocket closed.");
                }
            }
        }
    }
}
