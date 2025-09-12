using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class InvoiceDetailsItem
{
    public double Qty { get; set; }
    public double UnitPrice { get; set; }
    public double LineTotal => Qty * (UnitPrice);

    public InvoiceDetailsItem(double qty, double unitPrice)
    {
        Qty = qty;
        UnitPrice = unitPrice;
    }
}