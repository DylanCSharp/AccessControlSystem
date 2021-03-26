using AccessControlSystem.Models;
using Azure.Storage.Blobs;
using Grpc.Core;
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
        readonly AccessControl _context;
        readonly IConfiguration _config;
        readonly IWebHostEnvironment _hosting;

        static string name;
        static string reason;
        static string whom;

        public VisitorController(AccessControl context, IConfiguration configuration, IWebHostEnvironment hosting)
        {
            _context = context;
            _config = configuration;
            _hosting = hosting;
        }

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
        public IActionResult Index(string strName, string strReason, string strWhom)
        {
            try
            {
                //Making sure they input the values for later user
                if (strName != "" && strReason != "" && strWhom != "")
                {
                    name = strName;
                    reason = strReason;
                    whom = strWhom;

                    return RedirectToAction("Picture", "Visitor");
                }
                else
                {
                    ViewBag.Error = "Some values have not been entered, please try again";
                    return View();
                }
            }
            catch(Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Picture()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Picture(IFormFile file)
        {
            try
            {
                //Making sure all variables have values
                if (name != null && reason != null && whom != null && file != null)
                {
                    if (ModelState.IsValid)
                    {
                        DateTime date = DateTime.Now;

                        string connectionString = _config.GetConnectionString("AccessBlobStorage");
                        string fileName = Guid.NewGuid() + file.FileName;
                        string containerName = "visitorcontainer";

                        BlobContainerClient container = new BlobContainerClient(connectionString, containerName);

                        BlobClient blob = container.GetBlobClient(fileName);

                       
                        string uploadFolder = Path.Combine(_hosting.WebRootPath, "img");

                        string filePath = Path.Combine(uploadFolder, fileName);

                        //uploading the image to the image folder within the project
                        FileStream stream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(stream);
                        stream.Close();

                        //uploading that file from the folder path to blob storage
                        using (var fileStream = System.IO.File.OpenRead(filePath))
                        {
                            await blob.UploadAsync(fileStream);
                        }

                        SqlConnection conn = new SqlConnection(_config.GetConnectionString("AccessControlDatabase"));

                        await conn.OpenAsync();

                        string blobUrl = "https://cvserverstorage.blob.core.windows.net/visitorcontainer/" + fileName + "";

                        //adding blob url and visitor values to db
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
                else
                {
                    ViewBag.Error = "Something went wrong. Please try again";
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult VisitorsLog()
        {
            return View(_context.VisitorsLogs);
        }
    }
}
