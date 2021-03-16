using AccessControlSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AccessControlSystem.Controllers
{
    public class VisitorController : Controller
    {
        AccessControl _context;
        IHostEnvironment webHostEnvironment;

        public VisitorController(AccessControl context, IHostEnvironment webHost)
        {
            _context = context;
            webHostEnvironment = webHost;
        }

        public IActionResult Index()
        {
            return View(_context.Employees);
        }


        [HttpPost]
        public IActionResult Index(string name, string reason, string whom, List<IFormFile> file)
        {
            return RedirectToAction("Index", "Home");
        }
   
        
    }
}
