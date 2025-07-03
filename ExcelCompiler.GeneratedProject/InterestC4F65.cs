using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class InterestC4F65
{
    public List<double> Deposit { get; set; }
    public Dictionary<Int32,Double> _totalAtMemoization { get; set; } = new Dictionary<Int32,Double>();

    public double InterestAt(Int32 counter) => 0.015 / 12 * TotalAt(counter - 1);
    public double TotalAt(Int32 counter)
    {
        Int32 key = counter;
        if (_totalAtMemoization.ContainsKey(key))
        {
            return _totalAtMemoization[key];
        }

        if (Equals(counter, 0))
        {
            return 10000;
        }

        double result = TotalAt(counter - 1) + InterestAt(counter - 0) + Deposit[counter - 1];
        _totalAtMemoization.Add(key, result);
        return result;
    }

    public InterestC4F65(List<double> deposit)
    {
        Deposit = deposit;
    }
}