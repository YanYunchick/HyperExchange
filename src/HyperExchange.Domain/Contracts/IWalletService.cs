using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperExchange.Domain.Contracts;

public interface IWalletService
{
    Task<List<(string CurrencyCode, decimal totalPrice)>> CalculateTotalBalanceAsync();
}
