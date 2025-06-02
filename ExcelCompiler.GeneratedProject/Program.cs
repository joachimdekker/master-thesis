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
        double monthlyBudgetReportE9 = new List<double>
        {
            monthlyBudgetReportE14,
            monthlyBudgetReportE15,
            monthlyBudgetReportE16
        }.Sum();
        double monthlyBudgetReportE8 = tBLMonthlyExpenses.Select(t => t.ActualCost).Sum();
        double monthlyBudgetReportE7 = monthlyBudgetReportE9 - monthlyBudgetReportE8;
        double monthlyBudgetReportD14 = 6000;
        double monthlyBudgetReportD15 = 1000;
        double monthlyBudgetReportD16 = 2500;
        double monthlyBudgetReportD9 = new List<double>
        {
            monthlyBudgetReportD14,
            monthlyBudgetReportD15,
            monthlyBudgetReportD16
        }.Sum();
        double monthlyBudgetReportD8 = tBLMonthlyExpenses.Select(t => t.ProjectedCost).Sum();
        double monthlyBudgetReportD7 = monthlyBudgetReportD9 - monthlyBudgetReportD8;
        double monthlyBudgetReportF7 = monthlyBudgetReportE7 - monthlyBudgetReportD7;
        double monthlyBudgetReportF8 = monthlyBudgetReportE8 - monthlyBudgetReportD8;
        double monthlyBudgetReportF9 = monthlyBudgetReportE9 - monthlyBudgetReportD9;
        double monthlyBudgetReportF10 = new List<double>
        {
            monthlyBudgetReportF7,
            monthlyBudgetReportF8,
            monthlyBudgetReportF9
        }.Sum();
        double savingsE3 = 10000;
        double savingsI2 = 0.025;
        double savingsI3 = savingsI2 / 12;
        double savingsC4 = savingsI3 * savingsE3;
        double savingsD4 = 500;
        double savingsE4 = savingsE3 + savingsC4 + savingsD4;
        double savingsC5 = savingsI3 * savingsE4;
        double savingsD5 = 500;
        double savingsE5 = savingsE4 + savingsC5 + savingsD5;
        double savingsC6 = savingsI3 * savingsE5;
        double savingsD6 = 500;
        double savingsE6 = savingsE5 + savingsC6 + savingsD6;
        double savingsC7 = savingsI3 * savingsE6;
        double savingsD7 = 500;
        double savingsE7 = savingsE6 + savingsC7 + savingsD7;
        double savingsC8 = savingsI3 * savingsE7;
        double savingsD8 = 500;
        double savingsE8 = savingsE7 + savingsC8 + savingsD8;
        double savingsC9 = savingsI3 * savingsE8;
        double savingsD9 = 500;
        double savingsE9 = savingsE8 + savingsC9 + savingsD9;
        double savingsC10 = savingsI3 * savingsE9;
        double savingsD10 = 500;
        double savingsE10 = savingsE9 + savingsC10 + savingsD10;
        double savingsC11 = savingsI3 * savingsE10;
        double savingsD11 = 500;
        double savingsE11 = savingsE10 + savingsC11 + savingsD11;
        double savingsC12 = savingsI3 * savingsE11;
        double savingsD12 = 500;
        double savingsE12 = savingsE11 + savingsC12 + savingsD12;
        double savingsC13 = savingsI3 * savingsE12;
        double savingsD13 = 500;
        double savingsE13 = savingsE12 + savingsC13 + savingsD13;
        double savingsC14 = savingsI3 * savingsE13;
        double savingsD14 = 500;
        double savingsE14 = savingsE13 + savingsC14 + savingsD14;
        double savingsC15 = savingsI3 * savingsE14;
        double savingsD15 = 500;
        double savingsE15 = savingsE14 + savingsC15 + savingsD15;
        double savingsC16 = savingsI3 * savingsE15;
        double savingsD16 = 500;
        double savingsE16 = savingsE15 + savingsC16 + savingsD16;
        double savingsC17 = savingsI3 * savingsE16;
        double savingsD17 = 500;
        double savingsE17 = savingsE16 + savingsC17 + savingsD17;
        double savingsC18 = savingsI3 * savingsE17;
        double savingsD18 = 500;
        double savingsE18 = savingsE17 + savingsC18 + savingsD18;
        double savingsC19 = savingsI3 * savingsE18;
        double savingsD19 = 500;
        double savingsE19 = savingsE18 + savingsC19 + savingsD19;
        double savingsC20 = savingsI3 * savingsE19;
        double savingsD20 = 500;
        double savingsE20 = savingsE19 + savingsC20 + savingsD20;
        double savingsC21 = savingsI3 * savingsE20;
        double savingsD21 = 500;
        double savingsE21 = savingsE20 + savingsC21 + savingsD21;
        double savingsC22 = savingsI3 * savingsE21;
        double savingsD22 = 500;
        double savingsE22 = savingsE21 + savingsC22 + savingsD22;
        double savingsC23 = savingsI3 * savingsE22;
        double savingsD23 = 500;
        double savingsE23 = savingsE22 + savingsC23 + savingsD23;
        double savingsC24 = savingsI3 * savingsE23;
        double savingsD24 = 500;
        double savingsE24 = savingsE23 + savingsC24 + savingsD24;
        double savingsC25 = savingsI3 * savingsE24;
        double savingsD25 = 500;
        double savingsE25 = savingsE24 + savingsC25 + savingsD25;
        double savingsC26 = savingsI3 * savingsE25;
        double savingsD26 = 500;
        double savingsE26 = savingsE25 + savingsC26 + savingsD26;
        double savingsC27 = savingsI3 * savingsE26;
        double savingsD27 = 500;
        double savingsE27 = savingsE26 + savingsC27 + savingsD27;
        double savingsC28 = savingsI3 * savingsE27;
        double savingsD28 = 500;
        double savingsE28 = savingsE27 + savingsC28 + savingsD28;
        double savingsC29 = savingsI3 * savingsE28;
        double savingsD29 = 500;
        double savingsE29 = savingsE28 + savingsC29 + savingsD29;
        double savingsC30 = savingsI3 * savingsE29;
        double savingsD30 = 500;
        double savingsE30 = savingsE29 + savingsC30 + savingsD30;
        double savingsC31 = savingsI3 * savingsE30;
        double savingsD31 = 500;
        double savingsE31 = savingsE30 + savingsC31 + savingsD31;
        double savingsC32 = savingsI3 * savingsE31;
        double savingsD32 = 500;
        double savingsE32 = savingsE31 + savingsC32 + savingsD32;
        double savingsC33 = savingsI3 * savingsE32;
        double savingsD33 = 500;
        double savingsE33 = savingsE32 + savingsC33 + savingsD33;
        double savingsC34 = savingsI3 * savingsE33;
        double savingsD34 = 500;
        double savingsE34 = savingsE33 + savingsC34 + savingsD34;
        double savingsC35 = savingsI3 * savingsE34;
        double savingsD35 = 500;
        double savingsE35 = savingsE34 + savingsC35 + savingsD35;
        double savingsC36 = savingsI3 * savingsE35;
        double savingsD36 = 500;
        double savingsE36 = savingsE35 + savingsC36 + savingsD36;
        double savingsC37 = savingsI3 * savingsE36;
        double savingsD37 = 500;
        double savingsE37 = savingsE36 + savingsC37 + savingsD37;
        double savingsC38 = savingsI3 * savingsE37;
        double savingsD38 = 500;
        double savingsE38 = savingsE37 + savingsC38 + savingsD38;
        double savingsC39 = savingsI3 * savingsE38;
        double savingsD39 = 500;
        double savingsE39 = savingsE38 + savingsC39 + savingsD39;
        double savingsC40 = savingsI3 * savingsE39;
        double savingsD40 = 500;
        double savingsE40 = savingsE39 + savingsC40 + savingsD40;
        double savingsC41 = savingsI3 * savingsE40;
        double savingsD41 = 500;
        double savingsE41 = savingsE40 + savingsC41 + savingsD41;
        double savingsC42 = savingsI3 * savingsE41;
        double savingsD42 = 500;
        double savingsE42 = savingsE41 + savingsC42 + savingsD42;
        double savingsC43 = savingsI3 * savingsE42;
        double savingsD43 = 500;
        double savingsE43 = savingsE42 + savingsC43 + savingsD43;
        double savingsC44 = savingsI3 * savingsE43;
        double savingsD44 = 500;
        double savingsE44 = savingsE43 + savingsC44 + savingsD44;
        double savingsC45 = savingsI3 * savingsE44;
        double savingsD45 = 500;
        double savingsE45 = savingsE44 + savingsC45 + savingsD45;
        double savingsC46 = savingsI3 * savingsE45;
        double savingsD46 = 500;
        double savingsE46 = savingsE45 + savingsC46 + savingsD46;
        double savingsC47 = savingsI3 * savingsE46;
        double savingsD47 = 500;
        double savingsE47 = savingsE46 + savingsC47 + savingsD47;
        double savingsC48 = savingsI3 * savingsE47;
        double savingsD48 = 500;
        double savingsE48 = savingsE47 + savingsC48 + savingsD48;
        double savingsC49 = savingsI3 * savingsE48;
        double savingsD49 = 500;
        double savingsE49 = savingsE48 + savingsC49 + savingsD49;
        double savingsC50 = savingsI3 * savingsE49;
        double savingsD50 = 500;
        double savingsE50 = savingsE49 + savingsC50 + savingsD50;
        double savingsC51 = savingsI3 * savingsE50;
        double savingsD51 = 500;
        double savingsE51 = savingsE50 + savingsC51 + savingsD51;
        double savingsC52 = savingsI3 * savingsE51;
        double savingsD52 = 500;
        double savingsE52 = savingsE51 + savingsC52 + savingsD52;
        double savingsC53 = savingsI3 * savingsE52;
        double savingsD53 = 500;
        double savingsE53 = savingsE52 + savingsC53 + savingsD53;
        double savingsC54 = savingsI3 * savingsE53;
        double savingsD54 = 500;
        double savingsE54 = savingsE53 + savingsC54 + savingsD54;
        double savingsC55 = savingsI3 * savingsE54;
        double savingsD55 = 500;
        double savingsE55 = savingsE54 + savingsC55 + savingsD55;
        double savingsC56 = savingsI3 * savingsE55;
        double savingsD56 = 500;
        double savingsE56 = savingsE55 + savingsC56 + savingsD56;
        double savingsC57 = savingsI3 * savingsE56;
        double savingsD57 = 500;
        double savingsE57 = savingsE56 + savingsC57 + savingsD57;
        double savingsC58 = savingsI3 * savingsE57;
        double savingsD58 = 500;
        double savingsE58 = savingsE57 + savingsC58 + savingsD58;
        double savingsC59 = savingsI3 * savingsE58;
        double savingsD59 = 500;
        double savingsE59 = savingsE58 + savingsC59 + savingsD59;
        double savingsC60 = savingsI3 * savingsE59;
        double savingsD60 = 500;
        double savingsE60 = savingsE59 + savingsC60 + savingsD60;
        double savingsC61 = savingsI3 * savingsE60;
        double savingsD61 = 500;
        double savingsE61 = savingsE60 + savingsC61 + savingsD61;
        double savingsC62 = savingsI3 * savingsE61;
        double savingsD62 = 500;
        double savingsE62 = savingsE61 + savingsC62 + savingsD62;
        double savingsC63 = savingsI3 * savingsE62;
        double savingsD63 = 500;
        double savingsE63 = savingsE62 + savingsC63 + savingsD63;
        double savingsC68 = new List<double>
        {
            savingsD4,
            savingsD5,
            savingsD6,
            savingsD7,
            savingsD8,
            savingsD9,
            savingsD10,
            savingsD11,
            savingsD12,
            savingsD13,
            savingsD14,
            savingsD15,
            savingsD16,
            savingsD17,
            savingsD18,
            savingsD19,
            savingsD20,
            savingsD21,
            savingsD22,
            savingsD23,
            savingsD24,
            savingsD25,
            savingsD26,
            savingsD27,
            savingsD28,
            savingsD29,
            savingsD30,
            savingsD31,
            savingsD32,
            savingsD33,
            savingsD34,
            savingsD35,
            savingsD36,
            savingsD37,
            savingsD38,
            savingsD39,
            savingsD40,
            savingsD41,
            savingsD42,
            savingsD43,
            savingsD44,
            savingsD45,
            savingsD46,
            savingsD47,
            savingsD48,
            savingsD49,
            savingsD50,
            savingsD51,
            savingsD52,
            savingsD53,
            savingsD54,
            savingsD55,
            savingsD56,
            savingsD57,
            savingsD58,
            savingsD59,
            savingsD60,
            savingsD61,
            savingsD62,
            savingsD63
        }.Sum();
        double savingsC69 = savingsE63 - savingsE3 - savingsC68;
        double monthlyBudgetReportM20 = monthlyBudgetReportF10 + savingsC69 + tBLMonthlyExpenses.Select(t => t.Difference).Sum();
        return monthlyBudgetReportM20;
    }
}