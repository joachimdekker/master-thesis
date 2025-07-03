using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class TBL_MonthlyExpensesItem
{
    public double ProjectedCost { get; set; }
    public double ActualCost { get; set; }

    public TBL_MonthlyExpensesItem(double projectedCost, double actualCost)
    {
        ProjectedCost = projectedCost;
        ActualCost = actualCost;
    }
}