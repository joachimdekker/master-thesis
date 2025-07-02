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
        List<MonthlyBudgetReportC14F17Item> monthlyBudgetReportC14F17 = new List<MonthlyBudgetReportC14F17Item>
        {
            new MonthlyBudgetReportC14F17Item(6000, 5800),
            new MonthlyBudgetReportC14F17Item(1000, 2300),
            new MonthlyBudgetReportC14F17Item(2500, 1500)
        };
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
        InterestC4F65 interestC4F65 = new InterestC4F65(new List<double> { 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500 });
        double monthlyBudgetReportE9 = monthlyBudgetReportC14F17.Select(t => t.Actual).Sum();
        double interestF65 = interestC4F65.TotalAt(60);
        double interestF5 = 10000;
        double interestE6 = interestC4F65.Deposit[0];
        double interestE7 = interestC4F65.Deposit[1];
        double interestE8 = interestC4F65.Deposit[2];
        double interestE9 = interestC4F65.Deposit[3];
        double interestE10 = interestC4F65.Deposit[4];
        double interestE11 = interestC4F65.Deposit[5];
        double interestE12 = interestC4F65.Deposit[6];
        double interestE13 = interestC4F65.Deposit[7];
        double interestE14 = interestC4F65.Deposit[8];
        double interestE15 = interestC4F65.Deposit[9];
        double interestE16 = interestC4F65.Deposit[10];
        double interestE17 = interestC4F65.Deposit[11];
        double interestE18 = interestC4F65.Deposit[12];
        double interestE19 = interestC4F65.Deposit[13];
        double interestE20 = interestC4F65.Deposit[14];
        double interestE21 = interestC4F65.Deposit[15];
        double interestE22 = interestC4F65.Deposit[16];
        double interestE23 = interestC4F65.Deposit[17];
        double interestE24 = interestC4F65.Deposit[18];
        double interestE25 = interestC4F65.Deposit[19];
        double interestE26 = interestC4F65.Deposit[20];
        double interestE27 = interestC4F65.Deposit[21];
        double interestE28 = interestC4F65.Deposit[22];
        double interestE29 = interestC4F65.Deposit[23];
        double interestE30 = interestC4F65.Deposit[24];
        double interestE31 = interestC4F65.Deposit[25];
        double interestE32 = interestC4F65.Deposit[26];
        double interestE33 = interestC4F65.Deposit[27];
        double interestE34 = interestC4F65.Deposit[28];
        double interestE35 = interestC4F65.Deposit[29];
        double interestE36 = interestC4F65.Deposit[30];
        double interestE37 = interestC4F65.Deposit[31];
        double interestE38 = interestC4F65.Deposit[32];
        double interestE39 = interestC4F65.Deposit[33];
        double interestE40 = interestC4F65.Deposit[34];
        double interestE41 = interestC4F65.Deposit[35];
        double interestE42 = interestC4F65.Deposit[36];
        double interestE43 = interestC4F65.Deposit[37];
        double interestE44 = interestC4F65.Deposit[38];
        double interestE45 = interestC4F65.Deposit[39];
        double interestE46 = interestC4F65.Deposit[40];
        double interestE47 = interestC4F65.Deposit[41];
        double interestE48 = interestC4F65.Deposit[42];
        double interestE49 = interestC4F65.Deposit[43];
        double interestE50 = interestC4F65.Deposit[44];
        double interestE51 = interestC4F65.Deposit[45];
        double interestE52 = interestC4F65.Deposit[46];
        double interestE53 = interestC4F65.Deposit[47];
        double interestE54 = interestC4F65.Deposit[48];
        double interestE55 = interestC4F65.Deposit[49];
        double interestE56 = interestC4F65.Deposit[50];
        double interestE57 = interestC4F65.Deposit[51];
        double interestE58 = interestC4F65.Deposit[52];
        double interestE59 = interestC4F65.Deposit[53];
        double interestE60 = interestC4F65.Deposit[54];
        double interestE61 = interestC4F65.Deposit[55];
        double interestE62 = interestC4F65.Deposit[56];
        double interestE63 = interestC4F65.Deposit[57];
        double interestE64 = interestC4F65.Deposit[58];
        double interestE65 = interestC4F65.Deposit[59];
        double interestJ11 = new List<double>
        {
            interestE6,
            interestE7,
            interestE8,
            interestE9,
            interestE10,
            interestE11,
            interestE12,
            interestE13,
            interestE14,
            interestE15,
            interestE16,
            interestE17,
            interestE18,
            interestE19,
            interestE20,
            interestE21,
            interestE22,
            interestE23,
            interestE24,
            interestE25,
            interestE26,
            interestE27,
            interestE28,
            interestE29,
            interestE30,
            interestE31,
            interestE32,
            interestE33,
            interestE34,
            interestE35,
            interestE36,
            interestE37,
            interestE38,
            interestE39,
            interestE40,
            interestE41,
            interestE42,
            interestE43,
            interestE44,
            interestE45,
            interestE46,
            interestE47,
            interestE48,
            interestE49,
            interestE50,
            interestE51,
            interestE52,
            interestE53,
            interestE54,
            interestE55,
            interestE56,
            interestE57,
            interestE58,
            interestE59,
            interestE60,
            interestE61,
            interestE62,
            interestE63,
            interestE64,
            interestE65
        }.Sum();
        double interestJ12 = interestF65 - interestF5 - interestJ11;
        double monthlyBudgetReportJ7 = interestJ12;
        double monthlyBudgetReportE10 = monthlyBudgetReportJ7;
        double monthlyBudgetReportE8 = tBLMonthlyExpenses.Select(t => t.ActualCost).Sum();
        double monthlyBudgetReportE7 = monthlyBudgetReportE9 + monthlyBudgetReportE10 - monthlyBudgetReportE8;
        double monthlyBudgetReportD9 = monthlyBudgetReportC14F17.Select(t => t.Projected).Sum();
        double monthlyBudgetReportD10 = monthlyBudgetReportJ7;
        double monthlyBudgetReportD8 = tBLMonthlyExpenses.Select(t => t.ProjectedCost).Sum();
        double monthlyBudgetReportD7 = monthlyBudgetReportD9 + monthlyBudgetReportD10 - monthlyBudgetReportD8;
        double monthlyBudgetReportF7 = monthlyBudgetReportE7 - monthlyBudgetReportD7;
        return monthlyBudgetReportF7;
    }
}