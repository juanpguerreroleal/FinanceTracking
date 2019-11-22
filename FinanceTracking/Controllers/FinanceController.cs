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
using ChartJSCore.Models;
using StackExchange.Redis;
using ChartJSCore.Helpers;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using FinanceTracking.Models.ViewModels;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanceTracking.Controllers
{
    [Authorize]
    public class FinanceController : Controller
    {
        public readonly UserManager<IdentityUser> _userManager;
        public readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _env;
        public FinanceController(UserManager<IdentityUser> userManager, ApplicationDbContext db, IHostingEnvironment env)
        {
            _userManager = userManager;
            _db = db;
            _env = env;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            List<Expense> expenseList = _db.Expenses.Include(x => x.Category).Where(x => x.UserId == user.Id).ToList();
            return View(expenseList);
        }
        public IActionResult CreateExpense()
        {
            List<ExpenseCategory> expenseCategories = _db.ExpenseCategories.ToList();
            SelectList expenseCategoriesSelectList = new SelectList(expenseCategories, "Id", "Name");
            Expense expense = new Expense();
            expense.CreationDate = DateTime.Now;
            ViewBag.ExpenseCategoryId = expenseCategoriesSelectList;
            return View(expense);
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
        public async Task<IActionResult> EditExpense(int Id)
        {
            Expense expense = _db.Expenses.Where(x => x.Id == Id).FirstOrDefault();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;
            List<ExpenseCategory> expenseCategories = _db.ExpenseCategories.ToList();
            SelectList expenseCategoriesSelectList = new SelectList(expenseCategories, "Id", "Name");
            ViewBag.ExpenseCategoryId = expenseCategoriesSelectList;
            if (expense == null)
            {
                return RedirectToAction("Index");
            }
            return View(expense);
        }
        [HttpPost]
        public async Task<IActionResult> EditExpense(Expense expense)
        {
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                _db.Expenses.Update(expense);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Finance");
        }
        public async Task<IActionResult> Incomes()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            List<Income> incomesList = _db.Incomes.Include(x => x.IncomeSource).Where(x => x.IncomeSource.UserId == user.Id).ToList();
            return View(incomesList);
        }
        public async Task<IActionResult> CreateIncome()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;
            List<IncomeSource> incomeSources = _db.IncomeSources.Where(x => x.UserId == userId).ToList();
            if (incomeSources.Count == 0)
            {
                IncomeSource newIncomeSource = new IncomeSource
                {
                    UserId = userId,
                    Description = "Pago externo a salario",
                    CreationDate = DateTime.Now
                };
                await _db.IncomeSources.AddAsync(newIncomeSource);
                await _db.SaveChangesAsync();
                incomeSources = _db.IncomeSources.Where(x => x.UserId == userId).ToList();
            }
            SelectList incomeSourcesSelectList = new SelectList(incomeSources, "Id", "Description");
            Income income = new Income();
            income.CreationDate = DateTime.Now;
            ViewBag.incomeSources = incomeSourcesSelectList;
            return View(income);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIncome(Income income)
        {
            ModelState.Remove("UserId");
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;
            if (ModelState.IsValid)
            {
                await _db.Incomes.AddAsync(income);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Incomes));
            }
            return View(income);
        }
        public async Task<IActionResult> EditIncome(int Id)
        {
            Income income = _db.Incomes.Where(x => x.Id == Id).FirstOrDefault();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;
            List<IncomeSource> incomeSources = _db.IncomeSources.Where(x => x.UserId == userId).ToList();
            SelectList incomeSourcesSelectList = new SelectList(incomeSources, "Id", "Description");
            ViewBag.incomeSources = incomeSourcesSelectList;
            if (income == null)
            {
                return Redirect("Incomes");
            }
            return View(income);
        }
        [HttpPost]
        public async Task<IActionResult> EditIncome(Income income)
        {
            if (ModelState.IsValid)
            {
                _db.Incomes.Update(income);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Incomes", "Finance");
        }

        //Factibilidad de gastos
        public async Task<IActionResult> ExpensesFeseability() {
            var csvLocation = Path.Combine(Request.GetDisplayUrl().Replace("Finance/ExpensesFeseability", "") + "TensorModels/factibilityExpenseDataSet.csv");
            List<FeseabilityItem> csvItems = new List<FeseabilityItem>();
            List<ExpenseCategory> expenseCategories = _db.ExpenseCategories.ToList();
            SelectList expenseCategoriesSelectList = new SelectList(expenseCategories, "Id", "Name");
            ViewBag.ExpenseCategoryId = expenseCategoriesSelectList;
            using (var reader = new StreamReader("wwwroot/TensorModels/factibilityExpenseDataSetFixed.csv"))
            {
                var cont = 0;
                while (!reader.EndOfStream)
                {
                    FeseabilityItem item = new FeseabilityItem();
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (cont != 0)
                    {
                        item.CategoryId = Convert.ToInt32(values[0]);
                        item.TotalExpense = Convert.ToInt32(values[1]);
                        item.SalaryPerMonth = Convert.ToInt32(values[2]);
                        item.Age = Convert.ToInt32(values[3]);
                        item.StateId = Convert.ToInt32(values[4]);
                        item.TotalIncomes = Convert.ToInt32(values[5]);
                        item.Factibility = Convert.ToInt32(values[6]);
                        csvItems.Add(item);
                    }
                    cont++;
                }
            }
            ViewBag.TrainingItems = csvItems;
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user.Id;
            ViewBag.tsUrl = Path.Combine(Request.GetDisplayUrl().Replace("Finance/ExpensesFeseability", "") + "TensorModels/model.");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetUserExpensesAndIncomes(int CategoryId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user.Id;
            List<Expense> listaGastos = _db.Expenses.Where(x => x.UserId == userId && x.CreationDate > DateTime.Now.AddDays(-30) && x.ExpenseCategoryId == CategoryId).ToList();
            List<Income> listaEntradasDinero = _db.Incomes.Include(x => x.IncomeSource).Where(x => x.IncomeSource.UserId == userId && x.CreationDate > DateTime.Now.AddDays(-30)).ToList();
            Profile profile = await _db.Profiles.Where(x => x.UserId == userId).FirstOrDefaultAsync();
            FeseabilityItem item = new FeseabilityItem() {
                TotalExpense = 0,
                TotalIncomes = 0,
                Age = profile.Age,
                SalaryPerMonth = Convert.ToInt32(profile.Salary),
                StateId = profile.StateId
            };
            foreach(Expense expense in listaGastos)
            {
                item.TotalExpense += Convert.ToInt32(expense.Total);
            }
            foreach (Income income in listaEntradasDinero)
            {
                item.TotalIncomes += Convert.ToInt32(income.Total);
            }
            return Json(item);
        }
        //Estadisticas y clasificacion de nivel de gastos
        public async Task<IActionResult> ExpensesAndIncomes() {
            //Chart instance
            Chart chart = new Chart();
            Chart chartIncomes = new Chart();
            Chart chartBalance = new Chart();
            //Getting the user and setting the userId
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user.Id;
            //User expenses
            List<Expense> listagastos = _db.Expenses.Where(x => x.UserId == userId).ToList();
            //User profile
            List<Profile> listasalario = _db.Profiles.Where(x => x.UserId==userId).ToList();
            //User incomes
            List<Income> listaentradas = _db.Incomes.Include(x => x.IncomeSource).Where(x => x.IncomeSource.UserId == userId).ToList();
            //Setting the chart type
            chart.Type = Enums.ChartType.Line;
            chartIncomes.Type = Enums.ChartType.Line;
            chartBalance.Type = Enums.ChartType.Line;
            //Extracting the expenses data
            List<double> total = listagastos.Select(x => Convert.ToDouble(x.Total)).ToList();
            List<double> totalsalario = listasalario.Select(x => Convert.ToDouble(x.Salary)).ToList();
            double salariototal = totalsalario[0];
            ViewBag.salariototal = salariototal;
            //Extracting the incomes data
            List<double> totalIncomes = listagastos.Select(x => Convert.ToDouble(x.Total)).ToList();

            List<double> totalmes = new List<double>() { 0, 0, 0, 0, 0, 0,0, 0, 0, 0, 0, 0 };
            List<double> totalEntradasMes = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            List<double> totalBalance = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (Expense item in listagastos)
            {
                for (int i = 0; i <=11; i++)
                {
                    if (item.CreationDate.Month-1 == i)
                    {
                        var elemento = totalmes.ElementAt(i);
                        elemento += Convert.ToDouble(item.Total);
                        totalmes.RemoveAt(i);
                        totalmes.Insert(i, elemento);
                    }
                }
            }
            foreach (Income item in listaentradas)
            {
                for (int i =0; i <= 11; i++)
                {
                    if (item.CreationDate.Month-1 == i)
                    {
                        var elemento = totalEntradasMes.ElementAt(i);
                        elemento += Convert.ToDouble(item.Total);
                        totalEntradasMes.RemoveAt(i);
                        totalEntradasMes.Insert(i, elemento);
                    }
                }
            }
            for (int i = 0; i <= 11; i++)
            {
                double monthBalance = salariototal - totalmes.ElementAt(i) - totalEntradasMes.ElementAt(i);
                totalBalance.RemoveAt(i);
                totalBalance.Insert(i, monthBalance);
            }
            int indicemes = DateTime.Now.Month;
            double gastototal = totalmes[indicemes-1];
            double a = salariototal - gastototal;
            double porcentaje = (a * 100) / salariototal;
            ViewBag.porcentaje = porcentaje;
            //Data object for expenses
            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            //Data object for incomes
            ChartJSCore.Models.Data dataIncomes = new ChartJSCore.Models.Data();
            //Data object for balance
            ChartJSCore.Models.Data dataBalance = new ChartJSCore.Models.Data();
            List<string> months = new List<string>() { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            data.Labels = months;
            dataIncomes.Labels = months;
            dataBalance.Labels = months;
            //Dataset for expenses chart
            LineDataset dataset = new LineDataset()
            {
                Label = "Gastos por mes",
                Data = totalmes,
                Fill = "false",
                LineTension = 0.1,
                BackgroundColor = ChartColor.FromRgba(75, 192, 192, 0.4),
                BorderColor = ChartColor.FromRgb(75, 192, 192),
                BorderCapStyle = "butt",
                BorderDash = new List<int> { },
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };
            //Dataset for incomes chart
            LineDataset datasetIncomes = new LineDataset()
            {
                Label = "Entradas por mes",
                Data = totalEntradasMes,
                Fill = "false",
                LineTension = 0.1,
                BackgroundColor = ChartColor.FromRgba(75, 192, 192, 0.4),
                BorderColor = ChartColor.FromRgb(75, 192, 192),
                BorderCapStyle = "butt",
                BorderDash = new List<int> { },
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };
            //Dataset for balance chart
            LineDataset datasetBalance = new LineDataset()
            {
                Label = "Balance por mes",
                Data = totalBalance,
                Fill = "false",
                LineTension = 0.1,
                BackgroundColor = ChartColor.FromRgba(75, 192, 192, 0.4),
                BorderColor = ChartColor.FromRgb(75, 192, 192),
                BorderCapStyle = "butt",
                BorderDash = new List<int> { },
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            data.Datasets = new List<Dataset>();
            data.Datasets.Add(dataset);
            chart.Data = data;

            dataIncomes.Datasets = new List<Dataset>();
            dataIncomes.Datasets.Add(datasetIncomes);
            chartIncomes.Data = dataIncomes;

            dataBalance.Datasets = new List<Dataset>();
            dataBalance.Datasets.Add(datasetBalance);
            chartBalance.Data = dataBalance;

            //Sending data to the view
            ViewData["expenses"] = chart;
            ViewData["incomes"] = chartIncomes;
            ViewData["balance"] = chartBalance;

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
