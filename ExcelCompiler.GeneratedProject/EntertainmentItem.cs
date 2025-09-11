using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class EntertainmentItem
{
    public double Budget { get; set; }
    public double Actual { get; set; }

    public EntertainmentItem(double budget, double actual)
    {
        Budget = budget;
        Actual = actual;
    }
}