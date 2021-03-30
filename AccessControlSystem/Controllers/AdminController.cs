using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scrypt;
using AccessControlSystem.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace AccessControlSystem.Controllers
{
    public class AdminController : Controller
    {
        readonly AccessControl _context;
        readonly IConfiguration _config;

        private static string usernameStatic;
        private static int empid;

        public AdminController(AccessControl context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                return View(_context.Admins);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                ScryptEncoder scryptEncoder = new ScryptEncoder();

                usernameStatic = username;

                var admin = await _context.Admins.Where(x => x.AdminName.Equals(username)).FirstOrDefaultAsync();

                bool isAdmin = scryptEncoder.Compare(password, admin.AdminHash);

                //Checking admin login
                if (isAdmin)
                {
                    HttpContext.Session.SetString("LoggedInAdmin", admin.AdminId.ToString());
                    return View("Dashboard");
                }
                else
                {
                    ViewBag.Error = "Username and password do not match!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            try
            {
                if (HttpContext.Session.GetString("LoggedInAdmin") != null)
                {
                    return View();
                }
                else
                {
                    TempData["NotAdmin"] = "You need to be admin to access this page";
                    return RedirectToAction("Login", "Admin");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> VisitorLogs()
        {
            try
            {
                //only allowing admins to view this view
                var admin = await _context.Admins.Where(x => x.AdminName.Equals(usernameStatic)).FirstOrDefaultAsync();

                if (HttpContext.Session.GetString("LoggedInAdmin") != null)
                {
                    return View(_context.VisitorsLogs);
                }
                else
                {
                    TempData["NotAdmin"] = "You need to be admin to access this page";
                    return RedirectToAction("Login", "Admin");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult EmployeeList()
        {
            if (HttpContext.Session.GetString("LoggedInAdmin") != null)
            {
                return View(_context.Employees);
            }
            else
            {
                TempData["NotAdmin"] = "You need to be admin to access this page";
                return RedirectToAction("Login", "Admin");
            }
        }

        [HttpPost]
        public IActionResult EmployeeList(int id)
        {
            if (HttpContext.Session.GetString("LoggedInAdmin") != null)
            {
                empid = id;
                return RedirectToAction("EmployeeHistory", "Admin");
            }
            else
            {
                TempData["NotAdmin"] = "You need to be admin to access this page";
                return RedirectToAction("Login", "Admin");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeHistory()
        {
            try
            {
                if (HttpContext.Session.GetString("LoggedInAdmin") != null && empid != 0)
                {
                    var user = await _context.Employees.Where(x => x.EmployeeId.Equals(empid)).FirstOrDefaultAsync();
                    var userHistory = await _context.EmployeeLogs.Where(x => x.EmployeeId.Equals(empid)).FirstOrDefaultAsync();
                    ViewBag.EmployeeName = user.EmployeeName;
                    if (userHistory == null)
                    {
                        ViewBag.History = "No history";
                    }
                    return View(_context.EmployeeLogs.Where(x => x.EmployeeId.Equals(empid)));
                }
                else
                {
                    TempData["NotAdmin"] = "You need to be admin to access this page";
                    return RedirectToAction("Login", "Admin");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }
    }
}
