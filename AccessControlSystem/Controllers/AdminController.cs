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
using Microsoft.Data.SqlClient;

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
            try
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
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult EmployeeList(int id)
        {
            try
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
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
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

        [HttpGet]
        public IActionResult OvertimeRequests()
        {
            if (HttpContext.Session.GetString("LoggedInAdmin") != null)
            {
                return View(_context.OvertimeTickets);
            }
            else
            {
                TempData["NotAdmin"] = "You need to be admin to access this page";
                return RedirectToAction("Login", "Admin");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Overtime(bool ticketStatus)
        {
            DateTime date = DateTime.Now;

            int userID = Convert.ToInt32(HttpContext.Session.GetString("LoggedInAdmin"));

            var adminName = await _context.Admins.Where(x => x.AdminId.Equals(userID)).FirstOrDefaultAsync();

            string approved;
            if (ticketStatus == true)
            {
                approved = "APPROVED";
            }
            else
            {
                approved = "NOT APPROVED";
            }

            //SQL UPDATES STUFF
            SqlConnection conn = new SqlConnection();
            await conn.OpenAsync();

            string query = "UPDATE OVERTIME_TICKETS SET APPROVED = '"+approved+"' WHERE EMPLOYEE_ID = "+userID+" AND TICKET_DATE = '"+date.ToString("dd-MM-yyyy")+"'";

            SqlCommand command = new SqlCommand(query, conn);
            SqlDataReader dataReader = await command.ExecuteReaderAsync();

            await conn.CloseAsync();
            await command.DisposeAsync();
            await dataReader.CloseAsync();


            return RedirectToAction("Dashboard", "Admin");
        }

    }
}
