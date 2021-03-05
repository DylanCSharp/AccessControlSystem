using AccessControlSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AccessControlSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(int employee, int visitor)
        {
            if (employee == 1 && visitor == 0)
            {
                return RedirectToAction("Index", "Employee");
            }
            else
            {
                return RedirectToAction("Index", "Visitor");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }


    }
}
