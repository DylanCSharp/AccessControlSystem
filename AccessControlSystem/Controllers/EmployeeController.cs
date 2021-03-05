using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scrypt;
using Microsoft.Extensions.Configuration;
using AccessControlSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace AccessControlSystem.Controllers
{
    public class EmployeeController : Controller
    {
        IConfiguration _config;
        AccessControl _context;

        public EmployeeController(IConfiguration config, AccessControl context)
        {
            _config = config;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_context.Employees);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int one, int two, int three, int four, string name)
        {
            string passcode = one.ToString() + two.ToString() + three.ToString() + four.ToString();

            var user = await _context.Employees.Where(x => x.EmployeeName.Equals(name)).FirstOrDefaultAsync();

            ScryptEncoder scryptEncoder = new ScryptEncoder();
            bool isValid = scryptEncoder.Compare(passcode, user.EmployeeHashCode);

            if (isValid == true)
            {
                HttpContext.Session.SetString("LoggedInUser", user.EmployeeId.ToString());
                ViewBag.LoggedIn = "Welcome back " + user.EmployeeName;
                return View("CheckIn");
            }
            else
            {
                TempData["Invalid"] = "Sorry, this Employee Name and Passcode do not match!";
                return RedirectToAction("Index", "Employee");
            }
        }

        [HttpGet]
        public IActionResult CheckIn()
        {
            if (HttpContext.Session.GetString("LoggedInUser") != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Employee");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(int checkIn, int checkOut)
        {
            if (HttpContext.Session.GetString("LoggedInUser") != null)
            {
                DateTime date = DateTime.Now;

                string timeoutdummy = "00:00";

                int userID = Convert.ToInt32(HttpContext.Session.GetString("LoggedInUser"));

                var user = await _context.EmployeeLogs.Where(x => x.EmployeeId.Equals(userID)).FirstOrDefaultAsync();

                if (user.CheckInStatus == 1 && checkIn == 1 && user.DateLog.Contains(date.ToString("dd-MM-yyyy")))
                {
                    //user already signed in
                    TempData["AlreadyCheckedIn"] = "You are already checked in";
                    return RedirectToAction("Index", "Home");
                }
                else if (user.CheckInStatus == 0 && checkIn == 1 && !user.DateLog.Contains(date.ToString("dd-MM-yyyy")))
                {
                    //sign in user
                    //
                    SqlConnection conn = new SqlConnection(_config.GetConnectionString("AccessControlDatabase"));
                    await conn.OpenAsync();

                    string query = "INSERT INTO EMPLOYEE_LOG VALUES ("+userID+", '"+ date.ToShortTimeString() +"', '"+timeoutdummy+"', '"+date.ToString("dd-MM-yyyy")+"', "+checkIn+");";

                    SqlCommand command = new SqlCommand(query, conn);
                    SqlDataReader dataReader = await command.ExecuteReaderAsync();

                    await conn.CloseAsync();
                    await command.DisposeAsync();
                    await dataReader.CloseAsync();

                    TempData["CheckedIn"] = "You have been checked in";

                    return RedirectToAction("Index", "Home");

                }
                else if (user.CheckInStatus == 1 && checkOut == 0)
                {
                    //sign out user
                    TempData["SignOut"] = "You have been signed out";

                    //Change checked in status to 0
                    SqlConnection conn = new SqlConnection(_config.GetConnectionString("AccessControlDatabase"));
                    await conn.OpenAsync();

                    string query = "UPDATE EMPLOYEE_LOG SET CHECK_IN_STATUS = 0, TIME_OUT = '"+date.ToShortTimeString()+"' WHERE EMPLOYEE_LOG_NUMBER = "+user.EmployeeLogNumber+"";

                    SqlCommand command = new SqlCommand(query, conn);
                    SqlDataReader dataReader = await command.ExecuteReaderAsync();

                    await conn.CloseAsync();
                    await command.DisposeAsync();
                    await dataReader.CloseAsync();

                    return RedirectToAction("Index", "Home");
                }
                else if (user.CheckInStatus == 0 && checkOut == 0)
                {
                    //user already signed out
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Employee");
            }
        }
    }
}
