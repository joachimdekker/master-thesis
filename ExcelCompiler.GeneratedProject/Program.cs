using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Generated;
public class Program
{
    public double Main(List<GiftsItem> gifts, List<MealsItem> meals, List<PackagingItem> packaging, List<EntertainmentItem> entertainment, List<TravelItem> travel, List<MiscellaneousItem> miscellaneous)
    {
        double holidayBudgetPlannerD17 = gifts.Select(t => t.Budget).Sum();
        double holidayBudgetPlannerL17 = meals.Select(t => t.Budget).Sum();
        double holidayBudgetPlannerD28 = packaging.Select(t => t.Budget).Sum();
        double holidayBudgetPlannerL28 = entertainment.Select(t => t.Budget).Sum();
        double holidayBudgetPlannerD36 = travel.Select(t => t.Budget).Sum();
        double holidayBudgetPlannerL36 = miscellaneous.Select(t => t.Budget).Sum();
        double holidayBudgetPlannerN3 = holidayBudgetPlannerD17 + holidayBudgetPlannerL17 + holidayBudgetPlannerD28 + holidayBudgetPlannerL28 + holidayBudgetPlannerD36 + holidayBudgetPlannerL36;
        double holidayBudgetPlannerE17 = gifts.Select(t => t.Actual).Sum();
        double holidayBudgetPlannerM17 = meals.Select(t => t.Actual).Sum();
        double holidayBudgetPlannerE28 = packaging.Select(t => t.Actual).Sum();
        double holidayBudgetPlannerM28 = entertainment.Select(t => t.Actual).Sum();
        double holidayBudgetPlannerE36 = travel.Select(t => t.Actual).Sum();
        double holidayBudgetPlannerM36 = miscellaneous.Select(t => t.Actual).Sum();
        double holidayBudgetPlannerN4 = holidayBudgetPlannerE17 + holidayBudgetPlannerM17 + holidayBudgetPlannerE28 + holidayBudgetPlannerM28 + holidayBudgetPlannerE36 + holidayBudgetPlannerM36;
        double holidayBudgetPlannerN6 = holidayBudgetPlannerN3 - holidayBudgetPlannerN4;
        return holidayBudgetPlannerN6;
    }
}