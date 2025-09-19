#import "@preview/zebraw:0.5.5": *
#show: zebraw.with(lang: false)

#show figure: set text(0.8em)

#figure(
  zebraw(
    header: [Excelerate > Family Budget > Main],
    // highlight-lines: (
    //   (1, [The Fibonacci sequence is defined through the recurrence relation $F_n = F_(n-1) + F_(n-2)$\
    //   It can also be expressed in _closed form:_ $ F_n = round(1 / sqrt(5) phi.alt^n), quad
    //   phi.alt = (1 + sqrt(5)) / 2 $]),
    //   // Passing a range of line numbers in the array should begin with `..`
    //   ..range(9, 14),
    //   (13, [The first \#count numbers of the sequence.]),
    // ),
    lang: false,
    block-width: 100%,
    wrap: false,
    
    ```cs
    public double Main()
    {
        List<MonthlyBudgetReportC14F17Item> monthlyBudgetReportC14F17 = [new(6000, 5800), ..., new(2500, 1500)];
        double monthlyBudgetReportE9 = monthlyBudgetReportC14F17.Select(t => t.Actual).Sum();
        InterestC4F65 interestC4F65 = new([500, 500, ..., 500, 500]);
        double interestF65 = interestC4F65.TotalAt(60);
        double interestF5 = interestC4F65.TotalAt(0);
        double interestJ11 = interestC4F65.Deposit.Sum();
        double interestJ12 = interestF65 - (interestF5) - (interestJ11);
        List<TBL_MonthlyExpensesItem> tBL_MonthlyExpenses = [new(40, 40), new(0, 0), ..., new(0, 0), new(450, 450)];
        double monthlyBudgetReportE8 = tBL_MonthlyExpenses.Select(t => t.ActualCost).Sum();
        double monthlyBudgetReportE7 = monthlyBudgetReportE9 + interestJ12 - (monthlyBudgetReportE8);
        double monthlyBudgetReportD9 = monthlyBudgetReportC14F17.Select(t => t.Projected).Sum();
        double monthlyBudgetReportD8 = tBL_MonthlyExpenses.Select(t => t.ProjectedCost).Sum();
        double monthlyBudgetReportD7 = monthlyBudgetReportD9 + interestJ12 - (monthlyBudgetReportD8);
        double monthlyBudgetReportF7 = monthlyBudgetReportE7 - (monthlyBudgetReportD7);
        return monthlyBudgetReportF7;
    }
  ```,
))

#figure(
 zebraw(
   header: [Excelerate > Family Budget > Interest Chain],
    // highlight-lines: (
    //   (1, [The Fibonacci sequence is defined through the recurrence relation $F_n = F_(n-1) + F_(n-2)$\
    //   It can also be expressed in _closed form:_ $ F_n = round(1 / sqrt(5) phi.alt^n), quad
    //   phi.alt = (1 + sqrt(5)) / 2 $]),
    //   // Passing a range of line numbers in the array should begin with `..`
    //   ..range(9, 14),
    //   (13, [The first \#count numbers of the sequence.]),
    // ),
    lang: false,
    block-width: 100%,
    wrap: true,
    
    ```cs
  public class InterestC4F65
  {
      public List<double> Deposit { get; set; }
      public Dictionary<int,double> _totalAtMemoization { get; set; } = new Dictionary<int,double>();
  
      public double InterestAt(int counter) => 0.015 / (12) * (TotalAt(counter - (1)));
      public double TotalAt(int counter)
      {
          int key = counter;
          if (_totalAtMemoization.ContainsKey(key))
          {
              return _totalAtMemoization[key];
          }
  
          if (counter == 0)
          {
              return 10000;
          }
  
          double result = TotalAt(counter - (1)) + InterestAt(counter - (0)) + Deposit[counter - (1)];
          _totalAtMemoization.Add(key, result);
          return result;
      }
  
      public InterestC4F65(List<double> deposit)
      {
          Deposit = deposit;
      }
  }
  ```,
  
  ),
  caption: []
)<full-code:discussion:fbm-new>

#figure(
  zebraw(
   header: [Basic Compiler > Family Budget > Main],
    // highlight-lines: (
    //   (1, [The Fibonacci sequence is defined through the recurrence relation $F_n = F_(n-1) + F_(n-2)$\
    //   It can also be expressed in _closed form:_ $ F_n = round(1 / sqrt(5) phi.alt^n), quad
    //   phi.alt = (1 + sqrt(5)) / 2 $]),
    //   // Passing a range of line numbers in the array should begin with `..`
    //   ..range(9, 14),
    //   (13, [The first \#count numbers of the sequence.]),
    // ),
    lang: false,
    block-width: 100%,
    wrap: true,
  ```cs
  public double Main()
  {
      List<double> monthlyBudgetReportE9List = 
      [
        5800,
        2300,
        1500
      ]
      double monthlyBudgetReportE9 = monthlyBudgetReportE9List.Sum();
      double interestJ9 = 0.015 / (12);
      double interestD6 = interestJ9 * (10000);
      double interestF6 = 10000 + interestD6 + 500;
      double interestD7 = interestJ9 * (interestF6);
      double interestF7 = interestF6 + interestD7 + 500;
      double interestD8 = interestJ9 * (interestF7);
      double interestF8 = interestF7 + interestD8 + 500;
      double interestD9 = interestJ9 * (interestF8);
      double interestF9 = interestF8 + interestD9 + 500;
      ...
      double interestD63 = interestJ9 * (interestF62);
      double interestF63 = interestF62 + interestD63 + 500;
      double interestD64 = interestJ9 * (interestF63);
      double interestF64 = interestF63 + interestD64 + 500;
      double interestD65 = interestJ9 * (interestF64);
      double interestF65 = interestF64 + interestD65 + 500;
      List<double> interestJ11List =
      [
          0,
          500,
          500,
          ...
          500,
          500
      ];
      double interestJ11 = interestJ11List.Sum();
      double interestJ12 = interestF65 - (10000) - (interestJ11);
      List<double> monthlyBudgetReportE8List = 
      [
          40,
          0,
          ...
          0,
          450
      ];
      double monthlyBudgetReportE8 = monthlyBudgetReportE8List.Sum();
      double monthlyBudgetReportE7 = monthlyBudgetReportE9 + interestJ12 - (monthlyBudgetReportE8);
      List<double> monthlyBudgetReportD9List = [
        6000,
        1000,
        2500
      ];
      double monthlyBudgetReportD9 = monthlyBudgetReportD9List.Sum();
      List<double> monthlyBudgetReportD8List = 
      [ 
          40,
          0,
          ...
          0,
          450
      ];
      double monthlyBudgetReportD8 = monthlyBudgetReportD8List.Sum();
      double monthlyBudgetReportD7 = monthlyBudgetReportD9 + interestJ12 - (monthlyBudgetReportD8);
      double monthlyBudgetReportF7 = monthlyBudgetReportE7 - (monthlyBudgetReportD7);
      return monthlyBudgetReportF7;
  }
  ```),
  
)<full-code:discussion:fbm-old>


#figure(
  zebraw(
   header: [Excelerate > Actuarial Example],
    // highlight-lines: (
    //   (1, [The Fibonacci sequence is defined through the recurrence relation $F_n = F_(n-1) + F_(n-2)$\
    //   It can also be expressed in _closed form:_ $ F_n = round(1 / sqrt(5) phi.alt^n), quad
    //   phi.alt = (1 + sqrt(5)) / 2 $]),
    //   // Passing a range of line numbers in the array should begin with `..`
    //   ..range(9, 14),
    //   (13, [The first \#count numbers of the sequence.]),
    // ),
    lang: false,
    block-width: 100%,
    wrap: true,
  
```cs 
    public double Main(List<TableRenteparameter_Psi_RA1C100Item> renteparameter_Psi_RA1C100, List<TableRenteparameter_phi_R_NLA1D100Item> renteparameter_phi_R_NLA1D100)
    {
        double renteparameter_phi_R_NLB1 = renteparameter_phi_R_NLA1D100[0].Column1;
        double renteparameter_Psi_RA1 = renteparameter_Psi_RA1C100[0].Column0;
        double renteparameter_Psi_RB1 = renteparameter_Psi_RA1C100[0].Column1;
        double renteparameter_Psi_RC1 = renteparameter_Psi_RA1C100[0].Column2;
        double voorbeeldRTSE4 = Math.Exp(-(1) / (1) * (renteparameter_phi_R_NLB1 + 0.03296211605999014 * (renteparameter_Psi_RA1) + 0.014349731117662046 * (renteparameter_Psi_RB1) + 0.010609776508980583 * (renteparameter_Psi_RC1))) - (1);
        double voorbeeldRTSF4 = 100000 * (1 + voorbeeldRTSE4);
        double renteparameter_phi_R_NLB2 = renteparameter_phi_R_NLA1D100[1].Column1;
        double renteparameter_Psi_RA2 = renteparameter_Psi_RA1C100[1].Column0;
        double renteparameter_Psi_RB2 = renteparameter_Psi_RA1C100[1].Column1;
        double renteparameter_Psi_RC2 = renteparameter_Psi_RA1C100[1].Column2;
        double voorbeeldRTSE5 = Math.Exp(-(1) / (2) * (renteparameter_phi_R_NLB2 + 0.03296211605999014 * (renteparameter_Psi_RA2) + 0.014349731117662046 * (renteparameter_Psi_RB2) + 0.010609776508980583 * (renteparameter_Psi_RC2))) - (1);
        ...(97 more)
        double renteparameter_phi_R_NLB100 = renteparameter_phi_R_NLA1D100[99].Column1;
        double renteparameter_Psi_RA100 = renteparameter_Psi_RA1C100[99].Column0;
        double renteparameter_Psi_RB100 = renteparameter_Psi_RA1C100[99].Column1;
        double renteparameter_Psi_RC100 = renteparameter_Psi_RA1C100[99].Column2;
        double voorbeeldRTSE103 = Math.Exp(-(1) / (100) * (renteparameter_phi_R_NLB100 + 0.03296211605999014 * (renteparameter_Psi_RA100) + 0.014349731117662046 * (renteparameter_Psi_RB100) + 0.010609776508980583 * (renteparameter_Psi_RC100))) - (1);
        double voorbeeldRTSF103 = voorbeeldRTSF102 * (1 + voorbeeldRTSE103);
        double voorbeeldRTSI1 = voorbeeldRTSF103 - (100000);
        return voorbeeldRTSI1;
    }
}
```
))

#figure(
  zebraw(
   header: [Excelerate > Withdrawal Calculator Chain],
    lang: false,
    block-width: 100%,
    wrap: true,
  
```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class WithdrawalCalculatorD26G287
{
    public List<double> AdditionalWithdrawal { get; set; }
    public double WithdrawalAmountBaseCase { get; set; }
    public double BalanceBaseCase { get; set; }
    public Dictionary<int,double> _withdrawalAmountAtMemoization { get; set; } = new();
    public Dictionary<int,double> _balanceAtMemoization { get; set; } = new();

    public double InterestEarnedAt(int counter) => BalanceAt(counter - (1)) - (WithdrawalAmountAt(counter - (1))) * (0.04d / (12d));
    public double WithdrawalAmountAt(int counter)
    {
        int key = counter;
        if (_withdrawalAmountAtMemoization.ContainsKey(key))
        {
            return _withdrawalAmountAtMemoization[key];
        }

        if (Equals(counter, 0))
        {
            return WithdrawalAmountBaseCase;
        }

        double result = Math.Min(BalanceAt(counter - (1)) + InterestEarnedAt(counter - (0)), 1d + 0.025d / (12d) * (WithdrawalAmountAt(counter - (1))));
        _withdrawalAmountAtMemoization.Add(key, result);
        return result;
    }

    public double BalanceAt(int counter)
    {
        int key = counter;
        if (_balanceAtMemoization.ContainsKey(key))
        {
            return _balanceAtMemoization[key];
        }

        if (Equals(counter, 0))
        {
            return BalanceBaseCase;
        }

        double result = BalanceAt(counter - (1)) - (WithdrawalAmountAt(counter - (0))) - (AdditionalWithdrawal[counter - (1)]) + InterestEarnedAt(counter - (0));
        _balanceAtMemoization.Add(key, result);
        return result;
    }

    public WithdrawalCalculatorD26G287(List<double> additionalWithdrawal, double withdrawalAmountBaseCase, double balanceBaseCase)
    {
        AdditionalWithdrawal = additionalWithdrawal;
        WithdrawalAmountBaseCase = withdrawalAmountBaseCase;
        BalanceBaseCase = balanceBaseCase;
    }
}```
))

