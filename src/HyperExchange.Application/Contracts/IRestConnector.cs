using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyperExchange.Domain.Models;

namespace HyperExchange.Application.Contracts;

public interface IRestConnector
{
    Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount, CancellationToken cancellationToken);
    Task<IEnumerable<Candle>> GetCandleSeriesAsync(
        string pair, 
        int periodInSec, 
        CancellationToken cancellationToken,
        DateTimeOffset? from, 
        DateTimeOffset? to = null, 
        long? count = 0);
}
