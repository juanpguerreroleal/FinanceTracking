using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceTracking.Models
{
    public class Income
    {
        public int Id { get; set; }
        public IncomeSource IncomeSource { get; set; }
        [ForeignKey("IncomeSourceId")]
        public int IncomeSourceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Total { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
