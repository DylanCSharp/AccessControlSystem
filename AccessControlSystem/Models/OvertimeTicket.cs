using System;
using System.Collections.Generic;

#nullable disable

namespace AccessControlSystem.Models
{
    public partial class OvertimeTicket
    {
        public int TicketNum { get; set; }
        public string EmployeeName { get; set; }
        public string OvertimeReason { get; set; }
        public int OvertimeHours { get; set; }
        public string Declaration { get; set; }
        public string TicketApproved { get; set; }
        public string ApprovedByWhom { get; set; }
    }
}
