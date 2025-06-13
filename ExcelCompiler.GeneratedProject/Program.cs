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
        double monthlyBudgetReportE15 = 5800;
        double monthlyBudgetReportE16 = 2300;
        double monthlyBudgetReportE17 = 1500;
        double monthlyBudgetReportE9 = new List<double>
        {
            monthlyBudgetReportE15,
            monthlyBudgetReportE16,
            monthlyBudgetReportE17
        }.Sum();
        double interestF5 = 10000;
        double interestJ6 = 0.015;
        double interestJ9 = interestJ6 / 12;
        double interestD6 = interestJ9 * interestF5;
        double interestE6 = 500;
        double interestF6 = interestF5 + interestD6 + interestE6;
        double interestD7 = interestJ9 * interestF6;
        double interestE7 = 500;
        double interestF7 = interestF6 + interestD7 + interestE7;
        double interestD8 = interestJ9 * interestF7;
        double interestE8 = 500;
        double interestF8 = interestF7 + interestD8 + interestE8;
        double interestD9 = interestJ9 * interestF8;
        double interestE9 = 500;
        double interestF9 = interestF8 + interestD9 + interestE9;
        double interestD10 = interestJ9 * interestF9;
        double interestE10 = 500;
        double interestF10 = interestF9 + interestD10 + interestE10;
        double interestD11 = interestJ9 * interestF10;
        double interestE11 = 500;
        double interestF11 = interestF10 + interestD11 + interestE11;
        double interestD12 = interestJ9 * interestF11;
        double interestE12 = 500;
        double interestF12 = interestF11 + interestD12 + interestE12;
        double interestD13 = interestJ9 * interestF12;
        double interestE13 = 500;
        double interestF13 = interestF12 + interestD13 + interestE13;
        double interestD14 = interestJ9 * interestF13;
        double interestE14 = 500;
        double interestF14 = interestF13 + interestD14 + interestE14;
        double interestD15 = interestJ9 * interestF14;
        double interestE15 = 500;
        double interestF15 = interestF14 + interestD15 + interestE15;
        double interestD16 = interestJ9 * interestF15;
        double interestE16 = 500;
        double interestF16 = interestF15 + interestD16 + interestE16;
        double interestD17 = interestJ9 * interestF16;
        double interestE17 = 500;
        double interestF17 = interestF16 + interestD17 + interestE17;
        double interestD18 = interestJ9 * interestF17;
        double interestE18 = 500;
        double interestF18 = interestF17 + interestD18 + interestE18;
        double interestD19 = interestJ9 * interestF18;
        double interestE19 = 500;
        double interestF19 = interestF18 + interestD19 + interestE19;
        double interestD20 = interestJ9 * interestF19;
        double interestE20 = 500;
        double interestF20 = interestF19 + interestD20 + interestE20;
        double interestD21 = interestJ9 * interestF20;
        double interestE21 = 500;
        double interestF21 = interestF20 + interestD21 + interestE21;
        double interestD22 = interestJ9 * interestF21;
        double interestE22 = 500;
        double interestF22 = interestF21 + interestD22 + interestE22;
        double interestD23 = interestJ9 * interestF22;
        double interestE23 = 500;
        double interestF23 = interestF22 + interestD23 + interestE23;
        double interestD24 = interestJ9 * interestF23;
        double interestE24 = 500;
        double interestF24 = interestF23 + interestD24 + interestE24;
        double interestD25 = interestJ9 * interestF24;
        double interestE25 = 500;
        double interestF25 = interestF24 + interestD25 + interestE25;
        double interestD26 = interestJ9 * interestF25;
        double interestE26 = 500;
        double interestF26 = interestF25 + interestD26 + interestE26;
        double interestD27 = interestJ9 * interestF26;
        double interestE27 = 500;
        double interestF27 = interestF26 + interestD27 + interestE27;
        double interestD28 = interestJ9 * interestF27;
        double interestE28 = 500;
        double interestF28 = interestF27 + interestD28 + interestE28;
        double interestD29 = interestJ9 * interestF28;
        double interestE29 = 500;
        double interestF29 = interestF28 + interestD29 + interestE29;
        double interestD30 = interestJ9 * interestF29;
        double interestE30 = 500;
        double interestF30 = interestF29 + interestD30 + interestE30;
        double interestD31 = interestJ9 * interestF30;
        double interestE31 = 500;
        double interestF31 = interestF30 + interestD31 + interestE31;
        double interestD32 = interestJ9 * interestF31;
        double interestE32 = 500;
        double interestF32 = interestF31 + interestD32 + interestE32;
        double interestD33 = interestJ9 * interestF32;
        double interestE33 = 500;
        double interestF33 = interestF32 + interestD33 + interestE33;
        double interestD34 = interestJ9 * interestF33;
        double interestE34 = 500;
        double interestF34 = interestF33 + interestD34 + interestE34;
        double interestD35 = interestJ9 * interestF34;
        double interestE35 = 500;
        double interestF35 = interestF34 + interestD35 + interestE35;
        double interestD36 = interestJ9 * interestF35;
        double interestE36 = 500;
        double interestF36 = interestF35 + interestD36 + interestE36;
        double interestD37 = interestJ9 * interestF36;
        double interestE37 = 500;
        double interestF37 = interestF36 + interestD37 + interestE37;
        double interestD38 = interestJ9 * interestF37;
        double interestE38 = 500;
        double interestF38 = interestF37 + interestD38 + interestE38;
        double interestD39 = interestJ9 * interestF38;
        double interestE39 = 500;
        double interestF39 = interestF38 + interestD39 + interestE39;
        double interestD40 = interestJ9 * interestF39;
        double interestE40 = 500;
        double interestF40 = interestF39 + interestD40 + interestE40;
        double interestD41 = interestJ9 * interestF40;
        double interestE41 = 500;
        double interestF41 = interestF40 + interestD41 + interestE41;
        double interestD42 = interestJ9 * interestF41;
        double interestE42 = 500;
        double interestF42 = interestF41 + interestD42 + interestE42;
        double interestD43 = interestJ9 * interestF42;
        double interestE43 = 500;
        double interestF43 = interestF42 + interestD43 + interestE43;
        double interestD44 = interestJ9 * interestF43;
        double interestE44 = 500;
        double interestF44 = interestF43 + interestD44 + interestE44;
        double interestD45 = interestJ9 * interestF44;
        double interestE45 = 500;
        double interestF45 = interestF44 + interestD45 + interestE45;
        double interestD46 = interestJ9 * interestF45;
        double interestE46 = 500;
        double interestF46 = interestF45 + interestD46 + interestE46;
        double interestD47 = interestJ9 * interestF46;
        double interestE47 = 500;
        double interestF47 = interestF46 + interestD47 + interestE47;
        double interestD48 = interestJ9 * interestF47;
        double interestE48 = 500;
        double interestF48 = interestF47 + interestD48 + interestE48;
        double interestD49 = interestJ9 * interestF48;
        double interestE49 = 500;
        double interestF49 = interestF48 + interestD49 + interestE49;
        double interestD50 = interestJ9 * interestF49;
        double interestE50 = 500;
        double interestF50 = interestF49 + interestD50 + interestE50;
        double interestD51 = interestJ9 * interestF50;
        double interestE51 = 500;
        double interestF51 = interestF50 + interestD51 + interestE51;
        double interestD52 = interestJ9 * interestF51;
        double interestE52 = 500;
        double interestF52 = interestF51 + interestD52 + interestE52;
        double interestD53 = interestJ9 * interestF52;
        double interestE53 = 500;
        double interestF53 = interestF52 + interestD53 + interestE53;
        double interestD54 = interestJ9 * interestF53;
        double interestE54 = 500;
        double interestF54 = interestF53 + interestD54 + interestE54;
        double interestD55 = interestJ9 * interestF54;
        double interestE55 = 500;
        double interestF55 = interestF54 + interestD55 + interestE55;
        double interestD56 = interestJ9 * interestF55;
        double interestE56 = 500;
        double interestF56 = interestF55 + interestD56 + interestE56;
        double interestD57 = interestJ9 * interestF56;
        double interestE57 = 500;
        double interestF57 = interestF56 + interestD57 + interestE57;
        double interestD58 = interestJ9 * interestF57;
        double interestE58 = 500;
        double interestF58 = interestF57 + interestD58 + interestE58;
        double interestD59 = interestJ9 * interestF58;
        double interestE59 = 500;
        double interestF59 = interestF58 + interestD59 + interestE59;
        double interestD60 = interestJ9 * interestF59;
        double interestE60 = 500;
        double interestF60 = interestF59 + interestD60 + interestE60;
        double interestD61 = interestJ9 * interestF60;
        double interestE61 = 500;
        double interestF61 = interestF60 + interestD61 + interestE61;
        double interestD62 = interestJ9 * interestF61;
        double interestE62 = 500;
        double interestF62 = interestF61 + interestD62 + interestE62;
        double interestD63 = interestJ9 * interestF62;
        double interestE63 = 500;
        double interestF63 = interestF62 + interestD63 + interestE63;
        double interestD64 = interestJ9 * interestF63;
        double interestE64 = 500;
        double interestF64 = interestF63 + interestD64 + interestE64;
        double interestD65 = interestJ9 * interestF64;
        double interestE65 = 500;
        double interestF65 = interestF64 + interestD65 + interestE65;
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
        double monthlyBudgetReportD15 = 6000;
        double monthlyBudgetReportD16 = 1000;
        double monthlyBudgetReportD17 = 2500;
        double monthlyBudgetReportD9 = new List<double>
        {
            monthlyBudgetReportD15,
            monthlyBudgetReportD16,
            monthlyBudgetReportD17
        }.Sum();
        double monthlyBudgetReportD10 = monthlyBudgetReportJ7;
        double monthlyBudgetReportD8 = tBLMonthlyExpenses.Select(t => t.ProjectedCost).Sum();
        double monthlyBudgetReportD7 = monthlyBudgetReportD9 + monthlyBudgetReportD10 - monthlyBudgetReportD8;
        double monthlyBudgetReportF7 = monthlyBudgetReportE7 - monthlyBudgetReportD7;
        return monthlyBudgetReportF7;
    }
}