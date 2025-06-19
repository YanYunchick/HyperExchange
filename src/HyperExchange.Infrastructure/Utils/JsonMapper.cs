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
}
