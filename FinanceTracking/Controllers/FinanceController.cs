using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanceTracking.Controllers
{
    [Authorize]
    public class Finance : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
        //Factibilidad de gastos
        public IActionResult ExpensesFeseability() {
            return View();
        }
        //Estadisticas y clasificacion de nivel de gastos
        public IActionResult ExpensesAndIncomes() {
            return View();
        }
    }
}
