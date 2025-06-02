using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class TBLMonthlyExpensesItem
{
    public double ProjectedCost { get; set; }
    public double ActualCost { get; set; }
    public double Difference => ProjectedCost - ActualCost;

    public TBLMonthlyExpensesItem(double projectedCost, double actualCost)
    {
        ProjectedCost = projectedCost;
        ActualCost = actualCost;
    }
}