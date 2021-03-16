using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccessControlSystem.Models
{
    public class ImageModel
    {
        public IFormFile ProfileImage { get; set; }
    }
}
