using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class InterestC4F65
{
    public List<double> Deposit { get; set; }

    private readonly Dictionary<int, double> _totalCache = new();
    
    private readonly Dictionary<int, double> _interestCache = new();

    public double InterestAt(Int32 counter)
    {
        if (_interestCache.TryGetValue(counter, out var interest)) return interest;
        
        _interestCache[counter] = interest = 0.015 / 12 * TotalAt(counter - 1);
        return interest;
    }

    public double TotalAt(Int32 counter)
    {
        if (Equals(counter, 0))
        {
            return 10000;
        }

        if (_totalCache.TryGetValue(counter, out var total)) return total;

        _totalCache[counter] = total = TotalAt(counter - 1) + InterestAt(counter - 0) + Deposit[counter - 1];
        return total;
    }

    public InterestC4F65(List<double> deposit)
    {
        Deposit = deposit;
    }
}