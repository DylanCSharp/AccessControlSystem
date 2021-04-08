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
        public string TicketLogTime { get; set; }
        public string TicketHours { get; set; }
        public string Reason { get; set; }
        public string AskedPermission { get; set; }
        public string Declaration { get; set; }
        public string Approved { get; set; }
        public string ApprovedByWhom { get; set; }

        public virtual Employee Employee { get; set; }
    }
}
