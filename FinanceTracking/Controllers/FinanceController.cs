using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceTracking.Data;
using FinanceTracking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using StackExchange.Redis;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanceTracking.Controllers
{
    [Authorize]
    public class FinanceController : Controller
    {
        public readonly UserManager<IdentityUser> _userManager;
        public readonly ApplicationDbContext _db;
        public FinanceController(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            List<Expense> expenseList = _db.Expenses.Where(x => x.UserId == user.Id).ToList();
            return View(expenseList);
        }
        public IActionResult CreateExpense()
        {
            List<ExpenseCategory> expenseCategories = _db.ExpenseCategories.ToList();
            SelectList expenseCategoriesSelectList = new SelectList(expenseCategories, "Id", "Name");
            ViewBag.ExpenseCategoryId = expenseCategoriesSelectList;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(Expense expense)
        {
            ModelState.Remove("UserId");
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;
            expense.UserId = userId;
            if (ModelState.IsValid)
            {
                await _db.Expenses.AddAsync(expense);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(expense);
        }
        //Factibilidad de gastos
        public IActionResult ExpensesFeseability() {
            return View();
        }
        //Estadisticas y clasificacion de nivel de gastos
        public IActionResult ExpensesAndIncomes() {
            return View();
        }
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;
            Profile profile = _db.Profiles.Where(x => x.UserId == userId).FirstOrDefault();
            List<State> states = _db.States.ToList();
            SelectList statesSelectList = new SelectList(states, "Id", "Name");
            ViewBag.StateIds = statesSelectList;
            List<JobCategory> jobCategories = _db.JobCategories.ToList();
            SelectList jobCategoriesSelectList = new SelectList(jobCategories, "Id", "Name");
            ViewBag.JobCategoryIds = jobCategoriesSelectList;
            if (profile == null)
            {
                Profile newProfile = new Profile
                {
                    UserId = userId,
                    StateId = 1,
                    JobCategoryId = 1,
                    Age = 18,
                    Salary = 0
                };
                await _db.Profiles.AddAsync(newProfile);
                await _db.SaveChangesAsync();
                return View(newProfile);
            }
            return View(profile);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(Profile profile)
        {
            if (profile != null)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userId = user?.Id;
                profile.UserId = userId;
                _db.Profiles.Update(profile);
                await _db.SaveChangesAsync();
                return RedirectToAction("Profile");
            }
            return View();
        }
    }
}
