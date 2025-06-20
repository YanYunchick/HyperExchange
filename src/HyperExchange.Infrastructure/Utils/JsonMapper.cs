using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HyperExchange.Domain.Models;

namespace HyperExchange.Infrastructure.Utils;

public static class JsonMapper
{
    public static Trade ConvertJsonToTrade(string pair, JsonElement json)
    {
        return new Trade()
        {
            Id = json[0].GetInt64().ToString(),
            Time = DateTimeOffset.FromUnixTimeMilliseconds(json[1].GetInt64()),
            Amount = Math.Abs(json[2].GetDecimal()),
            Side = json[2].GetDecimal() > 0 ? "buy" : "sell",
            Price = json[3].GetDecimal(),
            Pair = pair
        };
    }

    public static Candle ConvertJsonToCandle(string pair, JsonElement json)
    {
        return new Candle()
        {
            OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(json[0].GetInt64()),
            OpenPrice = json[1].GetDecimal(),
            ClosePrice = json[2].GetDecimal(),
            HighPrice = json[3].GetDecimal(),
            LowPrice = json[4].GetDecimal(),
            TotalVolume = json[5].GetDecimal(),
            TotalPrice = json[5].GetDecimal() * json[2].GetDecimal(),
            Pair = pair
        };
    }

    public static IEnumerable<Trade> ConvertTradesToCollection(string pair, string json)
    {
        using var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;
        var tradeList = new List<Trade>();

        if (root.ValueKind == JsonValueKind.Array)
        {
            tradeList.AddRange(root.EnumerateArray().Select(tradeJson => ConvertJsonToTrade(pair, tradeJson)));
        }

        return tradeList;
    }

    public static IEnumerable<Candle> ConvertCandlesToCollection(string pair, string json)
    {
        using var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;
        var candleList = new List<Candle>();

        if (root.ValueKind == JsonValueKind.Array)
        {
            candleList.AddRange(root.EnumerateArray().Select(candleJson => ConvertJsonToCandle(pair, candleJson)));
        }

        return candleList;
    }
}
