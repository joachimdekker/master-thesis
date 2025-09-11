using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class PackagingItem
{
    public double Budget { get; set; }
    public double Actual { get; set; }

    public PackagingItem(double budget, double actual)
    {
        Budget = budget;
        Actual = actual;
    }
}