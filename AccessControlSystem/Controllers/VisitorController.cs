using AccessControlSystem.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
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
        public async Task<IActionResult> Picture(IFormFile file)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DateTime date = DateTime.Now;

                    string connectionString = _config.GetConnectionString("AccessBlobStorage");
                    string fileName = file.FileName;
                    string containerName = "visitorcontainer";

                    BlobContainerClient container = new BlobContainerClient(connectionString, containerName);

                    BlobClient blob = container.GetBlobClient(fileName);

                    string uploadFolder = Path.Combine(_hosting.WebRootPath, "img");

                    string filePath = Path.Combine(uploadFolder, fileName);

                    FileStream stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    stream.Close();

                    using (var fileStream = System.IO.File.OpenRead(filePath))
                    {
                        await blob.UploadAsync(fileStream);
                    }

                    SqlConnection conn = new SqlConnection(_config.GetConnectionString("AccessControlDatabase"));

                    await conn.OpenAsync();

                    string blobUrl = "https://cvserverstorage.blob.core.windows.net/visitorcontainer/" + fileName + "";

                    ////WORK TO GET BLOB STORAGE URL
                    string query = "INSERT INTO VISITORS_LOG VALUES ('" + name + "', '" + reason + "', '" + whom + "', '" + date.ToShortTimeString() + " on " + date.ToLongDateString() + "', '" + blobUrl + "');";

                    SqlCommand command = new SqlCommand(query, conn);
                    SqlDataReader dataReader = await command.ExecuteReaderAsync();

                    await command.DisposeAsync();
                    await dataReader.CloseAsync();
                    await conn.CloseAsync();


                    TempData["VisitorSuccess"] = "Your Visitor Ticket had been accepted, have a good day!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Upload unsuccessful. Please try again";
                    return View();
                }
            }
            catch (Exception)
            {
                TempData["Exception"] = "This picture already exists in storage, please rename your file before uploading it";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
