﻿using System;
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
        public async Task<IActionResult> ExpensesAndIncomes() {
            Chart chart = new Chart();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user.Id;
            List<Expense> listagastos = _db.Expenses.Where(x => x.UserId == userId).ToList();
            List<Profile> listasalario = _db.Profiles.Where(x => x.UserId==userId).ToList();

            chart.Type = Enums.ChartType.Line;

            List<double> total = listagastos.Select(x => Convert.ToDouble(x.Total)).ToList();
            List<double> totalsalario = listasalario.Select(x => Convert.ToDouble(x.Salary)).ToList();
            double salariototal = totalsalario[0];
            ViewBag.salariototal = salariototal;

            List<double> totalmes= new List<double>() { 0, 0, 0, 0, 0, 0,0, 0, 0, 0, 0, 0 };
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

            int indicemes = DateTime.Now.Month;
            double gastototal = totalmes[indicemes-1];
            double a = salariototal - gastototal;
            double porcentaje = (a * 100) / salariototal;
            ViewBag.porcentaje = porcentaje;

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = new List<string>() { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre"};

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

            data.Datasets = new List<Dataset>();
            data.Datasets.Add(dataset);

            chart.Data = data;

            ViewData["chart"] = chart;

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
