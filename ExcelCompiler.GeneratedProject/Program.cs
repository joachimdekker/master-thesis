using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class Program
{
    public double Main(double serviceInvoiceE28, List<InvoiceDetailsItem> invoiceDetails)
    {
        double serviceInvoiceE27 = invoiceDetails.Select(t => t.LineTotal).Sum();
        double serviceInvoiceE29 = serviceInvoiceE27 * (1 + serviceInvoiceE28);
        return serviceInvoiceE29;
    }
}