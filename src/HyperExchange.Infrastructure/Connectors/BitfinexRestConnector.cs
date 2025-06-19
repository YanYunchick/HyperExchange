using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyperExchange.Application.Contracts;
using HyperExchange.Domain.Models;

namespace HyperExchange.Infrastructure.Connectors;

public class BitfinexRestConnector : IRestConnector
{
    public Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        throw new NotImplementedException();
    }
}
