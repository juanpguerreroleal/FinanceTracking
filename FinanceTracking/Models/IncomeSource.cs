using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceTracking.Models
{
    public class IncomeSource
    {
        public int Id { get; set; }
        public IdentityUser User { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
