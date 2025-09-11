using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class GiftsItem
{
    public double Budget { get; set; }
    public double Actual { get; set; }

    public GiftsItem(double budget, double actual)
    {
        Budget = budget;
        Actual = actual;
    }
}