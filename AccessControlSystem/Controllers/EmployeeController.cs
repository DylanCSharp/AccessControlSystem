using AccessControlSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Scrypt;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AccessControlSystem.Controllers
{
    public class EmployeeController : Controller
    {
        readonly IConfiguration _config;
        readonly AccessControl _context;

        public EmployeeController(IConfiguration config, AccessControl context)
        {
            _config = config;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                return View(_context.Employees);
            }
            catch (Exception ex) 
            { 
                ViewBag.Error = ex.Message; 
                return View(); 
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(int one, int two, int three, int four, string name)
        {
            try
            {
                string passcode = one.ToString() + two.ToString() + three.ToString() + four.ToString();

                var user = await _context.Employees.Where(x => x.EmployeeName.Equals(name)).FirstOrDefaultAsync();

                ScryptEncoder scryptEncoder = new ScryptEncoder();

                bool isValid = scryptEncoder.Compare(passcode, user.EmployeeHashCode);

                if (isValid == true)
                {
                    HttpContext.Session.SetString("LoggedInUser", user.EmployeeId.ToString());

                    DateTime date = DateTime.Now;

                    string dummy = "18:00";
                    int checkInStatus = 0;

                    int userID = Convert.ToInt32(HttpContext.Session.GetString("LoggedInUser"));

                    //Checking if employee id and date exist
                    var employeeLog = await _context.EmployeeLogs.Where(x => x.EmployeeId.Equals(userID) && x.DateLog.Equals(date.ToString("dd-MM-yyyy"))).FirstOrDefaultAsync();

                    //if it does exist and check in status is checked in, welcome back
                    if (employeeLog != null && employeeLog.CheckInStatus == 1)
                    {
                        ViewBag.LoggedIn = "Welcome back " + user.EmployeeName;
                        return View("CheckIn");
                    }
                    //if it does exist and theyre checked out, update the exisiting employeelog number with the time in
                    else if (employeeLog != null && employeeLog.CheckInStatus == 0)
                    {
                        SqlConnection conn = new SqlConnection(_config.GetConnectionString("AccessControlDatabase"));
                        await conn.OpenAsync();

                        string query = "UPDATE EMPLOYEE_LOG SET TIME_OUT = '"+dummy+"' WHERE EMPLOYEE_ID = "+userID+" AND DATE_LOG = '"+date.ToString("dd-MM-yyyy")+"'";

                        SqlCommand command = new SqlCommand(query, conn);
                        SqlDataReader dataReader = await command.ExecuteReaderAsync();

                        await conn.CloseAsync();
                        await command.DisposeAsync();
                        await dataReader.CloseAsync();

                        ViewBag.LoggedIn = "Welcome back " + user.EmployeeName;
                        return View("CheckIn");
                    }
                    //if the user hasnt signed in for the day, insert a row into the table
                    else
                    {
                        SqlConnection conn = new SqlConnection(_config.GetConnectionString("AccessControlDatabase"));
                        await conn.OpenAsync();

                        string query = "INSERT INTO EMPLOYEE_LOG VALUES (" + userID + ", '" + date.ToShortTimeString() + "', '" + dummy + "', '" + date.ToString("dd-MM-yyyy") + "', " + checkInStatus + ");";

                        SqlCommand command = new SqlCommand(query, conn);
                        SqlDataReader dataReader = await command.ExecuteReaderAsync();

                        await conn.CloseAsync();
                        await command.DisposeAsync();
                        await dataReader.CloseAsync();

                        ViewBag.LoggedIn = "Welcome back " + user.EmployeeName;
                        return View("CheckIn");
                    }
                }
                else
                {
                    TempData["Invalid"] = "Sorry, Employee Name and Passcode Do Not Match!";
                    return RedirectToAction("Index", "Employee");
                }
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message.ToString();
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult CheckIn()
        {
            try
            {
                //making sure the user is signed in before checking in or out
                if (HttpContext.Session.GetString("LoggedInUser") != null)
                {
                    return View();
                }
                else
                {
                    return RedirectToAction("Index", "Employee");
                }
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message.ToString();
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(int checkIn, int checkOut)
        {
            try
            {
                if (HttpContext.Session.GetString("LoggedInUser") != null)
                {
                    DateTime date = DateTime.Now;

                    string timeoutdummy = "18:00";

                    int userID = Convert.ToInt32(HttpContext.Session.GetString("LoggedInUser"));

                    var username = await _context.Employees.Where(x => x.EmployeeId.Equals(userID)).FirstOrDefaultAsync();

                    var user = await _context.EmployeeLogs.Where(x => x.EmployeeId.Equals(userID) && x.DateLog.Equals(date.ToString("dd-MM-yyyy"))).FirstOrDefaultAsync();

                    //checking if the user is already checked in
                    if (user.CheckInStatus == 1 && checkIn == 1 && user.DateLog.Contains(date.ToString("dd-MM-yyyy")))
                    {
                        //user already signed in
                        ViewBag.AlreadySignedIn = "You are already checked in " + username.EmployeeName + "!";
                        return View();
                    }
                    //if the user wants to check in but theyre not checked in, update their row in the table where the date equals today for specific employee id
                    else if (user.CheckInStatus == 0 && checkIn == 1 && user.DateLog.Equals(date.ToString("dd-MM-yyyy")))
                    {
                        //sign in user
                        SqlConnection conn = new SqlConnection(_config.GetConnectionString("AccessControlDatabase"));
                        await conn.OpenAsync();

                        string query = "UPDATE EMPLOYEE_LOG SET TIME_OUT = '" + timeoutdummy + "', CHECK_IN_STATUS = " + checkIn + " WHERE EMPLOYEE_ID = " + userID + " AND DATE_LOG = '" + date.ToString("dd-MM-yyyy") + "'";

                        SqlCommand command = new SqlCommand(query, conn);
                        SqlDataReader dataReader = await command.ExecuteReaderAsync();

                        await conn.CloseAsync();
                        await command.DisposeAsync();
                        await dataReader.CloseAsync();

                        TempData["CheckedIn"] = "You have been checked in";

                        return RedirectToAction("Index", "Home");

                    }
                    //if user is checked in and wants to check out, check them out and update table
                    else if (user.CheckInStatus == 1 && checkOut == 1)
                    {
                        //sign out user
                        TempData["SignOut"] = "You have been checked out";

                        //Change checked in status to 0
                        SqlConnection conn = new SqlConnection(_config.GetConnectionString("AccessControlDatabase"));
                        await conn.OpenAsync();

                        string query = "UPDATE EMPLOYEE_LOG SET CHECK_IN_STATUS = 0, TIME_OUT = '" + date.ToShortTimeString() + "' WHERE EMPLOYEE_LOG_NUMBER = " + user.EmployeeLogNumber + "";

                        SqlCommand command = new SqlCommand(query, conn);
                        SqlDataReader dataReader = await command.ExecuteReaderAsync();

                        await conn.CloseAsync();
                        await command.DisposeAsync();
                        await dataReader.CloseAsync();

                        return RedirectToAction("Index", "Home");
                    }
                    //if the user is already checked out and they want to check out, tell them
                    else if (user.CheckInStatus == 0 && checkOut == 1)
                    {
                        //User already signed out
                        ViewBag.AlreadyCheckedOut = "You are already signed out " + username.EmployeeName + "!";
                        return View();
                    }
                    else
                    {

                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    TempData["SignIn"] = "You need to log in first";
                    return RedirectToAction("Index", "Employee");
                }
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message.ToString();
                return RedirectToAction("Index", "Employee");
            }
        }

        public IActionResult Logout()
        {
            try
            {
                //Clearing the session so that someone else can login
                HttpContext.Session.Clear();
                TempData["LoggedOut"] = "You have been logged out.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult OvertimeTicket()
        {
            return View();
        }

        [HttpPost]
        public IActionResult OvertimeTicket(string name, string reason, int extraHours, int declaration)
        {
            return View();
        }
    }
}
