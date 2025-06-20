using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HyperExchange.Application.Contracts;
using HyperExchange.Domain.Models;
using HyperExchange.Infrastructure.Utils;
using static System.Net.WebRequestMethods;

namespace HyperExchange.Infrastructure.Connectors;

public class BitfinexRestConnector : IRestConnector
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api-pub.bitfinex.com/v2";
    public BitfinexRestConnector(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(
        string pair, 
        int periodInSec, 
        CancellationToken cancellationToken,
        DateTimeOffset? from, 
        DateTimeOffset? to = null, 
        long? count = 0)
    {
        var period = PeriodConverter.ConvertBitfinexPeriod(periodInSec);
        var start = from?.ToUnixTimeMilliseconds();
        var url = $"{_baseUrl}/candles/trade:{period}:t{pair.ToUpper()}/hist?start={start}";
        if (to.HasValue)
            url += $"end={to?.ToUnixTimeMilliseconds()}&";
        if (count > 0)
            url += $"limit={count}&";

        var response = await _httpClient.GetStringAsync(url, cancellationToken);
        var candles = JsonMapper.ConvertCandlesToCollection(pair, response);

        return candles;
    }

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount, CancellationToken cancellationToken)
    {
        var url = $"{_baseUrl}/trades/t{pair}/hist?limit={maxCount}";

        var response = await _httpClient.GetStringAsync(url, cancellationToken);
        var trades = JsonMapper.ConvertTradesToCollection(pair, response);

        return trades;
    }

    public async Task<Dictionary<string, decimal>> GetTickersAsync(string[] pairs)
    {
        var url = $"{_baseUrl}/tickers?symbols={string.Join(",", pairs)}";
        var response = await _httpClient.GetStringAsync(url);
        var prices = JsonMapper.ConvertTickersPriceToCollection(response);

        return prices;
    }
}
