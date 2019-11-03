using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceTracking.Models
{
    public class Profile
    {
        public int Id { get; set; }
        //AspNetUserForeignKey
        public IdentityUser User { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }
        //StateForeignKey
        public State State { get; set; }
        [ForeignKey("StateId")]
        public int StateId { get; set; }
        //JobCategoryForeignKeys
        public JobCategory JobCategory { get; set; }
        [ForeignKey("JobCategoryId")]
        public int JobCategoryId { get; set; }
        public decimal Salary { get; set; }

    }
}
