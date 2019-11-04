using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceTracking.Models
{
    public class State
    {
        public int Id { get; set; }
        public Country Country { get; set; }
        [ForeignKey("CountryId")]
        [Required]
        public int CountryId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
