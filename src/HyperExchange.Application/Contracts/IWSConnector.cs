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
    // Добавил CancellationToken для всех методов.
    // Убрал неиспользуемые в API параметры

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

    // Добавил методы для подключения и отключения вебсокета
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
}
