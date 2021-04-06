using System;
using System.Collections.Generic;

#nullable disable

namespace AccessControlSystem.Models
{
    public partial class OvertimeTicket
    {
        public int TicketNum { get; set; }
        public int? EmployeeId { get; set; }
        public string TicketDate { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string Reason { get; set; }
        public string Declaration { get; set; }
        public string TicketApproved { get; set; }
        public string TicketApprovedByWhom { get; set; }

        public virtual Employee Employee { get; set; }
    }
}
