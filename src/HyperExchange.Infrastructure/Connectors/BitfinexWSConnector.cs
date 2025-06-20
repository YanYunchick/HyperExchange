using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HyperExchange.Application.Contracts;
using HyperExchange.Domain.Models;
using HyperExchange.Infrastructure.Utils;

namespace HyperExchange.Infrastructure.Connectors;

public class BitfinexWSConnector : IWSConnector, IDisposable
{
    private ClientWebSocket _webSocket = new ClientWebSocket();
    private readonly Uri _uri = new("wss://api-pub.bitfinex.com/ws/2");
    private Dictionary<string, int> _tradeChannels = new();
    private Dictionary<string, int> _candleChannels = new();
    public event Action<Trade>? NewBuyTrade;
    public event Action<Trade>? NewSellTrade;
    public event Action<Candle>? CandleSeriesProcessing;


    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.Connecting)
            return;
        await _webSocket.ConnectAsync(_uri, cancellationToken);
        _ = ReceiveMessagesAsync(cancellationToken);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, $"Stopped by Method: {nameof(DisconnectAsync)}", cancellationToken);
        }
    }

    public async Task SubscribeCandles(
        string pair, 
        int periodInSec, 
        CancellationToken cancellationToken)
    {
        var period = PeriodConverter.ConvertBitfinexPeriod(periodInSec);

        var requestMsg = JsonSerializer.Serialize(new
        {
            @event = "subscribe",
            channel = "candles",
            key = $"trade:{period}:t{pair}"
        });

        await SendMessageAsync(requestMsg, cancellationToken);
    }

    public async Task UnsubscribeCandles(string pair, CancellationToken cancellationToken)
    {
        if (_webSocket.State != WebSocketState.Open)
        {
            return;
        }

        if (!_candleChannels.TryGetValue(pair, out var channelId))
            throw new InvalidOperationException($"The channel for candle({pair}) not established.");

        var requestMsg = JsonSerializer.Serialize(new
        {
            @event = "unsubscribe",
            chanId = channelId
        });
        _candleChannels.Remove(pair);
        await SendMessageAsync(requestMsg, cancellationToken);
    }

    public async Task SubscribeTrades(string pair, CancellationToken cancellationToken)
    {
        if(_webSocket.State != WebSocketState.Open)
        {
            await ConnectAsync(cancellationToken);
        }

        var requestMsg = JsonSerializer.Serialize(new
        {
            @event = "subscribe",
            channel = "trades",
            symbol = $"t{pair.ToUpper()}"
        });

        await SendMessageAsync(requestMsg, cancellationToken);
    }

    public async Task UnsubscribeTrades(string pair, CancellationToken cancellationToken)
    {
        if (_webSocket.State != WebSocketState.Open)
        {
            return;
        }

        if (!_tradeChannels.TryGetValue(pair, out var channelId))
            throw new InvalidOperationException($"The channel for trade({pair}) not established.");

        var requestMsg = JsonSerializer.Serialize(new
        {
            @event = "unsubscribe",
            chanId = channelId
        });
        _tradeChannels.Remove(pair);
        await SendMessageAsync(requestMsg, cancellationToken);
    }

    private async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 32];
        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine(message);
                ProcessMessage(message);
            }
        }
    }

    private void ProcessMessage(string message)
    {
        using var json = JsonDocument.Parse(message);
        var root = json.RootElement;

        switch (root.ValueKind)
        {
            case JsonValueKind.Array:
                ProcessValuesMessage(root);
                break;
            case JsonValueKind.Object:
                ProcessInfoMessage(root);
                break;
        }
    }

    private void ProcessValuesMessage(JsonElement root)
    {
        if (root[0].TryGetInt32(out var channelId) && _tradeChannels.ContainsValue(channelId))
        {
            var pair = _tradeChannels.FirstOrDefault(x => x.Value == channelId).Key;
            if (root.GetArrayLength() > 2)
            {
                ProcessTradesUpdateMessage(root, pair);
            }
            else
            {
                ProcessTradesSnapshotMessage(root[1], pair);
            }
        }
        else if (_candleChannels.ContainsValue(channelId))
        {
            var pair = _candleChannels.FirstOrDefault(x => x.Value == channelId).Key;
            ProcessCandlesMessage(root[1], pair);
        }
    }

    private void ProcessTradesUpdateMessage(JsonElement root, string pair)
    {
        var trade = JsonMapper.ConvertJsonToTrade(pair, root[2]);
        InvokeTradeEvents(trade);   
    }

    private void ProcessTradesSnapshotMessage(JsonElement tradeData, string pair)
    {
        if (tradeData.ValueKind == JsonValueKind.Array)
        {
            foreach (var trade in tradeData
                         .EnumerateArray()
                         .Select(tradeJson => JsonMapper.ConvertJsonToTrade(pair, tradeJson)))
            {
                InvokeTradeEvents(trade);
            }
        }
    }

    private void InvokeTradeEvents(Trade trade)
    {
        switch (trade.Side)
        {
            case "buy":
                NewBuyTrade?.Invoke(trade);
                break;
            case "sell":
                NewSellTrade?.Invoke(trade);
                break;
        }
    }

    private void ProcessCandlesMessage(JsonElement candleData, string pair)
    {
        if (candleData.ValueKind == JsonValueKind.Array && candleData.GetArrayLength() > 0)
        {
            if (candleData[0].ValueKind == JsonValueKind.Array)
            {
                foreach (var candle in candleData
                            .EnumerateArray()
                            .Select(candleJson => JsonMapper.ConvertJsonToCandle(pair, candleJson)))
                {
                    CandleSeriesProcessing?.Invoke(candle);
                }
            }
            else
            {
                var candle = JsonMapper.ConvertJsonToCandle(pair, candleData);
                CandleSeriesProcessing?.Invoke(candle);
            }
        }
    }

    private void ProcessInfoMessage(JsonElement root)
    {
        if (root.TryGetProperty("event", out var eventProperty))
        {
            var eventType = eventProperty.GetString();

            if (eventType is "subscribed")
            {
                if (root.TryGetProperty("chanId", out var chanIdProperty) &&
                    root.TryGetProperty("channel", out var channelProperty))
                {
                    var chanId = chanIdProperty.GetInt32();
                    var channel = channelProperty.GetString();

                    switch (channel)
                    {
                        case "trades":
                            if (root.TryGetProperty("pair", out var pairProperty))
                                _tradeChannels[pairProperty.GetString()!] = chanId;
                            break;
                        case "candles":
                            if (root.TryGetProperty("key", out var keyProperty))
                            {
                                var candlePair = keyProperty.GetString()!.Split(':')[2];
                                _candleChannels[candlePair.Substring(1)] = chanId;
                            }
                            break;
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }
}
