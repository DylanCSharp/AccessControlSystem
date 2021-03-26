using System;
using System.Collections.Generic;

#nullable disable

namespace AccessControlSystem.Models
{
    public partial class Admin
    {
        public int AdminId { get; set; }
        public string AdminName { get; set; }
        public string AdminHash { get; set; }
    }
}
