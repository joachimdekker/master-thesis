using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class Program
{
    public double Main()
    {
        List<TBLMonthlyExpensesItem> tBLMonthlyExpenses = new List<TBLMonthlyExpensesItem>
        {
            new TBLMonthlyExpensesItem(40, 40),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(100, 100),
            new TBLMonthlyExpensesItem(50, 40),
            new TBLMonthlyExpensesItem(200, 150),
            new TBLMonthlyExpensesItem(50, 28),
            new TBLMonthlyExpensesItem(50, 30),
            new TBLMonthlyExpensesItem(0, 40),
            new TBLMonthlyExpensesItem(20, 50),
            new TBLMonthlyExpensesItem(30, 20),
            new TBLMonthlyExpensesItem(1000, 1200),
            new TBLMonthlyExpensesItem(100, 120),
            new TBLMonthlyExpensesItem(75, 100),
            new TBLMonthlyExpensesItem(25, 25),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(100, 100),
            new TBLMonthlyExpensesItem(45, 50),
            new TBLMonthlyExpensesItem(300, 400),
            new TBLMonthlyExpensesItem(200, 0),
            new TBLMonthlyExpensesItem(200, 150),
            new TBLMonthlyExpensesItem(1700, 1700),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(100, 100),
            new TBLMonthlyExpensesItem(60, 60),
            new TBLMonthlyExpensesItem(35, 39),
            new TBLMonthlyExpensesItem(40, 55),
            new TBLMonthlyExpensesItem(25, 22),
            new TBLMonthlyExpensesItem(25, 26),
            new TBLMonthlyExpensesItem(400, 400),
            new TBLMonthlyExpensesItem(400, 400),
            new TBLMonthlyExpensesItem(100, 100),
            new TBLMonthlyExpensesItem(200, 200),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(150, 140),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(150, 75),
            new TBLMonthlyExpensesItem(20, 25),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(200, 200),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(300, 300),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(100, 150),
            new TBLMonthlyExpensesItem(450, 400),
            new TBLMonthlyExpensesItem(300, 300),
            new TBLMonthlyExpensesItem(25, 25),
            new TBLMonthlyExpensesItem(100, 50),
            new TBLMonthlyExpensesItem(0, 0),
            new TBLMonthlyExpensesItem(450, 450)
        };
        double monthlyBudgetReportE14 = 5800;
        double monthlyBudgetReportE15 = 2300;
        double monthlyBudgetReportE16 = 1500;
        var monthlyBudgetReportE9 = new List<double>
        {
            monthlyBudgetReportE14,
            monthlyBudgetReportE15,
            monthlyBudgetReportE16
        }.Sum();
        var monthlyBudgetReportE8 = tBLMonthlyExpenses.Select(t => t.ActualCost).Sum();
        var monthlyBudgetReportE7 = monthlyBudgetReportE9 - monthlyBudgetReportE8;
        double monthlyBudgetReportD14 = 6000;
        double monthlyBudgetReportD15 = 1000;
        double monthlyBudgetReportD16 = 2500;
        var monthlyBudgetReportD9 = new List<double>
        {
            monthlyBudgetReportD14,
            monthlyBudgetReportD15,
            monthlyBudgetReportD16
        }.Sum();
        var monthlyBudgetReportD8 = tBLMonthlyExpenses.Select(t => t.ProjectedCost).Sum();
        var monthlyBudgetReportD7 = monthlyBudgetReportD9 - monthlyBudgetReportD8;
        var monthlyBudgetReportF7 = monthlyBudgetReportE7 - monthlyBudgetReportD7;
        var monthlyBudgetReportF8 = monthlyBudgetReportE8 - monthlyBudgetReportD8;
        var monthlyBudgetReportF9 = monthlyBudgetReportE9 - monthlyBudgetReportD9;
        var monthlyBudgetReportF10 = new List<double>
        {
            monthlyBudgetReportF7,
            monthlyBudgetReportF8,
            monthlyBudgetReportF9
        }.Sum();
        return monthlyBudgetReportF10;
    }
}