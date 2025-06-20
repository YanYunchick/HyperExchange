using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperExchange.Infrastructure.Utils;

public class PeriodConverter
{
    public static string ConvertBitfinexPeriod(int periodInSec)
    {
        return periodInSec switch
        {
            60 => "1m",          
            300 => "5m",        
            900 => "15m",        
            1800 => "30m",       
            3600 => "1h",        
            10800 => "3h",       
            21600 => "6h",       
            43200 => "12h",      
            86400 => "1D",       
            604800 => "1W",      
            1209600 => "14D",    
            2592000 => "1M",     
            _ => throw new ArgumentException("Invalid period for Bitfinex API.")
        };
    }
}
