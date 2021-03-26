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

        public AdminController(AccessControl context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(_context.Admins);
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            
            ScryptEncoder scryptEncoder = new ScryptEncoder();

            usernameStatic = username;

            var admin = await _context.Admins.Where(x => x.AdminName.Equals(username)).FirstOrDefaultAsync();

            bool isAdmin = scryptEncoder.Compare(password, admin.AdminHash);

            //Checking admin login
            if (isAdmin)
            {
                HttpContext.Session.SetString("LoggedInAdmin", admin.AdminId.ToString());
                return RedirectToAction("VisitorLogs", "Admin");
            }
            else
            {
                ViewBag.Error = "Username and password do not match!";
                return View();
            }   
        }

        [HttpGet]
        public async Task<IActionResult> VisitorLogs()
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
    }
}
