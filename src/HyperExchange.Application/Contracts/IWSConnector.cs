using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyperExchange.Domain.Models;

namespace HyperExchange.Application.Contracts;

public interface IWSConnector
{
    event Action<Trade> NewBuyTrade;
    event Action<Trade> NewSellTrade;
    Task SubscribeTrades(string pair, CancellationToken cancellationToken);
    Task UnsubscribeTrades(string pair, CancellationToken cancellationToken);

    event Action<Candle> CandleSeriesProcessing;
    Task SubscribeCandles(
        string pair, 
        int periodInSec,
        CancellationToken cancellationToken);
    Task UnsubscribeCandles(string pair, CancellationToken cancellationToken);

    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
}
