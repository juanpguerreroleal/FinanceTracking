using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceTracking.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public ExpenseCategory Category { get; set; }
        [ForeignKey("ExpenseCategoryId")]
        public int ExpenseCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Total { get; set; }
        public DateTime CreationDate { get; set; }
        public IdentityUser User { get; set; }
        [ForeignKey("UserId")]
        [Required]
        public string UserId { get; set; }
    }
}
