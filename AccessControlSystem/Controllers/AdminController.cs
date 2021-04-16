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

                if (isAdmin == true)
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
        public IActionResult EmployeeOvertime()
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
        public IActionResult EmployeeOvertime(int id)
        {
            try
            {
                if (HttpContext.Session.GetString("LoggedInAdmin") != null)
                {
                    empid = id;
                    return RedirectToAction("OvertimeRequests", "Admin");
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
        public async Task<IActionResult> OvertimeRequests()
        {
            try
            {
                if (HttpContext.Session.GetString("LoggedInAdmin") != null)
                {
                    var userHistory = await _context.OvertimeTickets.Where(x => x.EmployeeId.Equals(empid) && x.Approved.Equals("NOT YET")).FirstOrDefaultAsync();
                    if (userHistory == null)
                    {
                        ViewBag.History = "No Pending Overtime Requests for this Employee";
                    }
                    return View(_context.OvertimeTickets.Where(x => x.EmployeeId.Equals(empid) && x.Approved.Equals("NOT YET")));
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
        public async Task<IActionResult> OvertimeRequests(string ticketStatus, int ticketNumber)
        {
            try
            {
                DateTime date = DateTime.Now;

                int userID = Convert.ToInt32(HttpContext.Session.GetString("LoggedInAdmin"));

                var adminName = await _context.Admins.Where(x => x.AdminId.Equals(userID)).FirstOrDefaultAsync();

                string approved;
                if (ticketStatus.Equals("true"))
                {
                    approved = "APPROVED";
                }
                else
                {
                    approved = "DENIED";
                }

                //SQL UPDATES STUFF
                SqlConnection conn = new SqlConnection(_config.GetConnectionString("AccessControlDatabase"));
                await conn.OpenAsync();

                string query = "UPDATE OVERTIME_TICKETS SET APPROVED = '"+approved+"', APPROVED_BY_WHOM = '"+adminName.AdminName+"' WHERE TICKET_NUM = "+ticketNumber+"";

                SqlCommand command = new SqlCommand(query, conn);
                SqlDataReader dataReader = await command.ExecuteReaderAsync();

                await conn.CloseAsync();
                await command.DisposeAsync();
                await dataReader.CloseAsync();

                var user = await _context.Employees.Where(x => x.EmployeeId.Equals(empid)).FirstOrDefaultAsync();

                ViewBag.Successful = "Ticket Number " + ticketNumber + " for "+ user.EmployeeName +" has successfully been given a status of " + approved;
                return View(_context.OvertimeTickets.Where(x => x.EmployeeId.Equals(empid) && x.Approved.Equals("NOT YET")));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(_context.OvertimeTickets);
            }
        }

    }
}
