using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyperExchange.Application.Contracts;
using HyperExchange.Domain.Contracts;
using HyperExchange.Domain.Models;

namespace HyperExchange.Application.Services;

public class WalletService : IWalletService
{
    private readonly IRestConnector _restConnector;
    public WalletService(IRestConnector restConnector)
    {
        _restConnector = restConnector;
    }

    public async Task<List<(string CurrencyCode, decimal totalPrice)>> CalculateTotalBalanceAsync()
    {
        var prices = await _restConnector.GetTickersAsync(["tBTCUSD", "tXRPUSD", "tXMRUSD", "tDSHUSD"]);

        var wallet = new List<Currency>
        {
            new() { CurrencyCode = "BTC", Amount = 1 },
            new() { CurrencyCode = "XRP", Amount = 15000 },
            new() { CurrencyCode = "XMR", Amount = 50 },
            new() { CurrencyCode = "DSH", Amount = 30}
        };

        decimal totalInUSD = wallet.Sum(currency =>
        {
            return currency.Amount * prices[$"t{currency.CurrencyCode}USD"];
        });

        var balances = new List<(string CurrencyCode, decimal totalBalance)>() { ("USDT", totalInUSD)};

        wallet.ForEach(c => balances.Add((c.CurrencyCode, totalInUSD / prices[$"t{c.CurrencyCode}USD"])));

        return balances;
    }
}
