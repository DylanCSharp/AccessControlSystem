using AccessControlSystem.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AccessControlSystem.Controllers
{
    public class VisitorController : Controller
    {
        AccessControl _context;
        IConfiguration _config;
        IWebHostEnvironment _hosting;

        public static string name;
        public static string reason;
        public static string whom;

        public VisitorController(AccessControl context, IConfiguration configuration, IWebHostEnvironment hosting)
        {
            _context = context;
            _config = configuration;
            _hosting = hosting;
        }

        public IActionResult Index()
        {
            return View(_context.Employees);
        }


        [HttpPost]
        public IActionResult Index(string strName, string strReason, string strWhom)
        {
            name = strName;
            reason = strReason;
            whom = strWhom;

            return RedirectToAction("Picture", "Visitor");
        }

        [HttpGet]
        public IActionResult Picture()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Picture(ImageModel model)
        {
            DateTime date = DateTime.Now;

            string fileName = model.ProfileImage.FileName;

            BlobContainerClient container = new BlobContainerClient(_config.GetConnectionString("AccessBlobStorage"), "accesscontainer");

            BlobClient blob = container.GetBlobClient(fileName);

            string uploadFolder = Path.Combine(_hosting.WebRootPath, "img");

            string filePath = Path.Combine(uploadFolder, fileName);

            FileStream stream = new FileStream(filePath, FileMode.Create);
            await model.ProfileImage.CopyToAsync(stream);
            stream.Close();

            using (var fileStream = System.IO.File.OpenRead(filePath))
            {
                await blob.UploadAsync(fileStream);
            }

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("AccessControlDatabase"));

            await conn.OpenAsync();

            //WORK TO GET BLOB STORAGE URL
            string query = "INSERT INTO VISITORS_LOG VALUES ('" + name + "', '" + reason + "', '" + whom + "', '" + date.ToShortTimeString() + " on " + date.ToLongDateString() + "', '" + model.ProfileImage + "');";

            SqlCommand command = new SqlCommand(query, conn);
            SqlDataReader dataReader = await command.ExecuteReaderAsync();

            await command.DisposeAsync();
            await dataReader.CloseAsync();
            await conn.CloseAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
