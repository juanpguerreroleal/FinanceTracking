using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceTracking.Models.ViewModels
{
    public class FeseabilityItem
    {
        public int CategoryId { get; set; }
        public int TotalExpense { get; set; }
        public int SalaryPerMonth { get; set; }
        public int Age { get; set; }
        public int StateId { get; set; }
        public int TotalIncomes { get; set; }
        public int Factibility { get; set; }
    }
}
