using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperExchange.Domain.Models;

public class Currency
{
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
