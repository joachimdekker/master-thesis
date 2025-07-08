using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class Program
{
    public double Main(double monthlyBudgetReportE10)
    {
        List<MonthlyBudgetReportC14F17Item> monthlyBudgetReportC14F17 = new List<MonthlyBudgetReportC14F17Item>
        {
            new MonthlyBudgetReportC14F17Item(6000, 5800),
            new MonthlyBudgetReportC14F17Item(1000, 2300),
            new MonthlyBudgetReportC14F17Item(2500, 1500)
        };
        List<TBL_MonthlyExpensesItem> tBL_MonthlyExpenses = new List<TBL_MonthlyExpensesItem>
        {
            new TBL_MonthlyExpensesItem(40, 40),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(100, 100),
            new TBL_MonthlyExpensesItem(50, 40),
            new TBL_MonthlyExpensesItem(200, 150),
            new TBL_MonthlyExpensesItem(50, 28),
            new TBL_MonthlyExpensesItem(50, 30),
            new TBL_MonthlyExpensesItem(0, 40),
            new TBL_MonthlyExpensesItem(20, 50),
            new TBL_MonthlyExpensesItem(30, 20),
            new TBL_MonthlyExpensesItem(1000, 1200),
            new TBL_MonthlyExpensesItem(100, 120),
            new TBL_MonthlyExpensesItem(75, 100),
            new TBL_MonthlyExpensesItem(25, 25),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(100, 100),
            new TBL_MonthlyExpensesItem(45, 50),
            new TBL_MonthlyExpensesItem(300, 400),
            new TBL_MonthlyExpensesItem(200, 0),
            new TBL_MonthlyExpensesItem(200, 150),
            new TBL_MonthlyExpensesItem(1700, 1700),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(100, 100),
            new TBL_MonthlyExpensesItem(60, 60),
            new TBL_MonthlyExpensesItem(35, 39),
            new TBL_MonthlyExpensesItem(40, 55),
            new TBL_MonthlyExpensesItem(25, 22),
            new TBL_MonthlyExpensesItem(25, 26),
            new TBL_MonthlyExpensesItem(400, 400),
            new TBL_MonthlyExpensesItem(400, 400),
            new TBL_MonthlyExpensesItem(100, 100),
            new TBL_MonthlyExpensesItem(200, 200),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(150, 140),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(150, 75),
            new TBL_MonthlyExpensesItem(20, 25),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(200, 200),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(300, 300),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(100, 150),
            new TBL_MonthlyExpensesItem(450, 400),
            new TBL_MonthlyExpensesItem(300, 300),
            new TBL_MonthlyExpensesItem(25, 25),
            new TBL_MonthlyExpensesItem(100, 50),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(450, 450)
        };
        InterestC4F65 interestC4F65 = new InterestC4F65(new List<double> { 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500 });
        double monthlyBudgetReportE9 = monthlyBudgetReportC14F17.Select(t => t.Actual).Sum();
        double monthlyBudgetReportE8 = tBL_MonthlyExpenses.Select(t => t.ActualCost).Sum();
        double monthlyBudgetReportE7 = monthlyBudgetReportE9 + monthlyBudgetReportE10 - monthlyBudgetReportE8;
        double monthlyBudgetReportD9 = monthlyBudgetReportC14F17.Select(t => t.Projected).Sum();
        double interestF65 = interestC4F65.TotalAt(60);
        double interestF5 = interestC4F65.TotalAt(0);
        double interestJ11 = interestC4F65.Deposit.Sum();
        double interestJ12 = interestF65 - interestF5 - interestJ11;
        double monthlyBudgetReportJ7 = interestJ12;
        double monthlyBudgetReportD10 = monthlyBudgetReportJ7;
        double monthlyBudgetReportD8 = tBL_MonthlyExpenses.Select(t => t.ProjectedCost).Sum();
        double monthlyBudgetReportD7 = monthlyBudgetReportD9 + monthlyBudgetReportD10 - monthlyBudgetReportD8;
        double monthlyBudgetReportF7 = monthlyBudgetReportE7 - monthlyBudgetReportD7;
        return monthlyBudgetReportF7;
    }
}