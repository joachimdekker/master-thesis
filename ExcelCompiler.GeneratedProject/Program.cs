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
        double monthlyBudgetReportD15 = 6000;
        double monthlyBudgetReportD16 = 1000;
        double monthlyBudgetReportD17 = 2500;
        double monthlyBudgetReportE15 = 5800;
        double monthlyBudgetReportE16 = 2300;
        double monthlyBudgetReportE17 = 1500;
        List<MonthlyBudgetReportC14F17Item> monthlyBudgetReportC14F17 = new List<MonthlyBudgetReportC14F17Item>
        {
            new MonthlyBudgetReportC14F17Item(monthlyBudgetReportD15, monthlyBudgetReportE15),
            new MonthlyBudgetReportC14F17Item(monthlyBudgetReportD16, monthlyBudgetReportE16),
            new MonthlyBudgetReportC14F17Item(monthlyBudgetReportD17, monthlyBudgetReportE17)
        };
        double monthlyBudgetReportE9 = monthlyBudgetReportC14F17.Select(t => t.Actual).Sum();
        double monthlyExpensesE5 = 40;
        double monthlyExpensesE6 = 0;
        double monthlyExpensesE7 = 0;
        double monthlyExpensesE8 = 100;
        double monthlyExpensesE9 = 50;
        double monthlyExpensesE10 = 200;
        double monthlyExpensesE11 = 50;
        double monthlyExpensesE12 = 50;
        double monthlyExpensesE13 = 0;
        double monthlyExpensesE14 = 20;
        double monthlyExpensesE15 = 30;
        double monthlyExpensesE16 = 1000;
        double monthlyExpensesE17 = 100;
        double monthlyExpensesE18 = 75;
        double monthlyExpensesE19 = 25;
        double monthlyExpensesE22 = 100;
        double monthlyExpensesE23 = 45;
        double monthlyExpensesE24 = 300;
        double monthlyExpensesE25 = 200;
        double monthlyExpensesE26 = 200;
        double monthlyExpensesE27 = 1700;
        double monthlyExpensesE29 = 100;
        double monthlyExpensesE30 = 60;
        double monthlyExpensesE31 = 35;
        double monthlyExpensesE32 = 40;
        double monthlyExpensesE33 = 25;
        double monthlyExpensesE34 = 25;
        double monthlyExpensesE35 = 400;
        double monthlyExpensesE36 = 400;
        double monthlyExpensesE37 = 100;
        double monthlyExpensesE38 = 200;
        double monthlyExpensesE43 = 150;
        double monthlyExpensesE48 = 150;
        double monthlyExpensesE49 = 20;
        double monthlyExpensesE52 = 200;
        double monthlyExpensesE54 = 300;
        double monthlyExpensesE57 = 100;
        double monthlyExpensesE58 = 450;
        double monthlyExpensesE59 = 300;
        double monthlyExpensesE60 = 25;
        double monthlyExpensesE61 = 100;
        double monthlyExpensesE63 = 450;
        double monthlyExpensesF5 = 40;
        double monthlyExpensesF6 = 0;
        double monthlyExpensesF7 = 0;
        double monthlyExpensesF8 = 100;
        double monthlyExpensesF9 = 40;
        double monthlyExpensesF10 = 150;
        double monthlyExpensesF11 = 28;
        double monthlyExpensesF12 = 30;
        double monthlyExpensesF13 = 40;
        double monthlyExpensesF14 = 50;
        double monthlyExpensesF15 = 20;
        double monthlyExpensesF16 = 1200;
        double monthlyExpensesF17 = 120;
        double monthlyExpensesF18 = 100;
        double monthlyExpensesF19 = 25;
        double monthlyExpensesF22 = 100;
        double monthlyExpensesF23 = 50;
        double monthlyExpensesF24 = 400;
        double monthlyExpensesF26 = 150;
        double monthlyExpensesF27 = 1700;
        double monthlyExpensesF29 = 100;
        double monthlyExpensesF30 = 60;
        double monthlyExpensesF31 = 39;
        double monthlyExpensesF32 = 55;
        double monthlyExpensesF33 = 22;
        double monthlyExpensesF34 = 26;
        double monthlyExpensesF35 = 400;
        double monthlyExpensesF36 = 400;
        double monthlyExpensesF37 = 100;
        double monthlyExpensesF38 = 200;
        double monthlyExpensesF43 = 140;
        double monthlyExpensesF48 = 75;
        double monthlyExpensesF49 = 25;
        double monthlyExpensesF52 = 200;
        double monthlyExpensesF54 = 300;
        double monthlyExpensesF57 = 150;
        double monthlyExpensesF58 = 400;
        double monthlyExpensesF59 = 300;
        double monthlyExpensesF60 = 25;
        double monthlyExpensesF61 = 50;
        double monthlyExpensesF63 = 450;
        List<TBL_MonthlyExpensesItem> tBL_MonthlyExpenses = new List<TBL_MonthlyExpensesItem>
        {
            new TBL_MonthlyExpensesItem(monthlyExpensesE5, monthlyExpensesF5),
            new TBL_MonthlyExpensesItem(monthlyExpensesE6, monthlyExpensesF6),
            new TBL_MonthlyExpensesItem(monthlyExpensesE7, monthlyExpensesF7),
            new TBL_MonthlyExpensesItem(monthlyExpensesE8, monthlyExpensesF8),
            new TBL_MonthlyExpensesItem(monthlyExpensesE9, monthlyExpensesF9),
            new TBL_MonthlyExpensesItem(monthlyExpensesE10, monthlyExpensesF10),
            new TBL_MonthlyExpensesItem(monthlyExpensesE11, monthlyExpensesF11),
            new TBL_MonthlyExpensesItem(monthlyExpensesE12, monthlyExpensesF12),
            new TBL_MonthlyExpensesItem(monthlyExpensesE13, monthlyExpensesF13),
            new TBL_MonthlyExpensesItem(monthlyExpensesE14, monthlyExpensesF14),
            new TBL_MonthlyExpensesItem(monthlyExpensesE15, monthlyExpensesF15),
            new TBL_MonthlyExpensesItem(monthlyExpensesE16, monthlyExpensesF16),
            new TBL_MonthlyExpensesItem(monthlyExpensesE17, monthlyExpensesF17),
            new TBL_MonthlyExpensesItem(monthlyExpensesE18, monthlyExpensesF18),
            new TBL_MonthlyExpensesItem(monthlyExpensesE19, monthlyExpensesF19),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(monthlyExpensesE22, monthlyExpensesF22),
            new TBL_MonthlyExpensesItem(monthlyExpensesE23, monthlyExpensesF23),
            new TBL_MonthlyExpensesItem(monthlyExpensesE24, monthlyExpensesF24),
            new TBL_MonthlyExpensesItem(monthlyExpensesE25, 0),
            new TBL_MonthlyExpensesItem(monthlyExpensesE26, monthlyExpensesF26),
            new TBL_MonthlyExpensesItem(monthlyExpensesE27, monthlyExpensesF27),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(monthlyExpensesE29, monthlyExpensesF29),
            new TBL_MonthlyExpensesItem(monthlyExpensesE30, monthlyExpensesF30),
            new TBL_MonthlyExpensesItem(monthlyExpensesE31, monthlyExpensesF31),
            new TBL_MonthlyExpensesItem(monthlyExpensesE32, monthlyExpensesF32),
            new TBL_MonthlyExpensesItem(monthlyExpensesE33, monthlyExpensesF33),
            new TBL_MonthlyExpensesItem(monthlyExpensesE34, monthlyExpensesF34),
            new TBL_MonthlyExpensesItem(monthlyExpensesE35, monthlyExpensesF35),
            new TBL_MonthlyExpensesItem(monthlyExpensesE36, monthlyExpensesF36),
            new TBL_MonthlyExpensesItem(monthlyExpensesE37, monthlyExpensesF37),
            new TBL_MonthlyExpensesItem(monthlyExpensesE38, monthlyExpensesF38),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(monthlyExpensesE43, monthlyExpensesF43),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(monthlyExpensesE48, monthlyExpensesF48),
            new TBL_MonthlyExpensesItem(monthlyExpensesE49, monthlyExpensesF49),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(monthlyExpensesE52, monthlyExpensesF52),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(monthlyExpensesE54, monthlyExpensesF54),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(monthlyExpensesE57, monthlyExpensesF57),
            new TBL_MonthlyExpensesItem(monthlyExpensesE58, monthlyExpensesF58),
            new TBL_MonthlyExpensesItem(monthlyExpensesE59, monthlyExpensesF59),
            new TBL_MonthlyExpensesItem(monthlyExpensesE60, monthlyExpensesF60),
            new TBL_MonthlyExpensesItem(monthlyExpensesE61, monthlyExpensesF61),
            new TBL_MonthlyExpensesItem(0, 0),
            new TBL_MonthlyExpensesItem(monthlyExpensesE63, monthlyExpensesF63)
        };
        double monthlyBudgetReportE8 = tBL_MonthlyExpenses.Select(t => t.ActualCost).Sum();
        double monthlyBudgetReportE7 = monthlyBudgetReportE9 + monthlyBudgetReportE10 - monthlyBudgetReportE8;
        double monthlyBudgetReportD9 = monthlyBudgetReportC14F17.Select(t => t.Projected).Sum();
        double interestE6 = 500;
        double interestE7 = 500;
        double interestE8 = 500;
        double interestE9 = 500;
        double interestE10 = 500;
        double interestE11 = 500;
        double interestE12 = 500;
        double interestE13 = 500;
        double interestE14 = 500;
        double interestE15 = 500;
        double interestE16 = 500;
        double interestE17 = 500;
        double interestE18 = 500;
        double interestE19 = 500;
        double interestE20 = 500;
        double interestE21 = 500;
        double interestE22 = 500;
        double interestE23 = 500;
        double interestE24 = 500;
        double interestE25 = 500;
        double interestE26 = 500;
        double interestE27 = 500;
        double interestE28 = 500;
        double interestE29 = 500;
        double interestE30 = 500;
        double interestE31 = 500;
        double interestE32 = 500;
        double interestE33 = 500;
        double interestE34 = 500;
        double interestE35 = 500;
        double interestE36 = 500;
        double interestE37 = 500;
        double interestE38 = 500;
        double interestE39 = 500;
        double interestE40 = 500;
        double interestE41 = 500;
        double interestE42 = 500;
        double interestE43 = 500;
        double interestE44 = 500;
        double interestE45 = 500;
        double interestE46 = 500;
        double interestE47 = 500;
        double interestE48 = 500;
        double interestE49 = 500;
        double interestE50 = 500;
        double interestE51 = 500;
        double interestE52 = 500;
        double interestE53 = 500;
        double interestE54 = 500;
        double interestE55 = 500;
        double interestE56 = 500;
        double interestE57 = 500;
        double interestE58 = 500;
        double interestE59 = 500;
        double interestE60 = 500;
        double interestE61 = 500;
        double interestE62 = 500;
        double interestE63 = 500;
        double interestE64 = 500;
        double interestE65 = 500;
        InterestC4F65 interestC4F65 = new InterestC4F65(new List<double> { interestE6, interestE7, interestE8, interestE9, interestE10, interestE11, interestE12, interestE13, interestE14, interestE15, interestE16, interestE17, interestE18, interestE19, interestE20, interestE21, interestE22, interestE23, interestE24, interestE25, interestE26, interestE27, interestE28, interestE29, interestE30, interestE31, interestE32, interestE33, interestE34, interestE35, interestE36, interestE37, interestE38, interestE39, interestE40, interestE41, interestE42, interestE43, interestE44, interestE45, interestE46, interestE47, interestE48, interestE49, interestE50, interestE51, interestE52, interestE53, interestE54, interestE55, interestE56, interestE57, interestE58, interestE59, interestE60, interestE61, interestE62, interestE63, interestE64, interestE65 });
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