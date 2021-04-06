using System;
using System.Collections.Generic;

#nullable disable

namespace AccessControlSystem.Models
{
    public partial class Employee
    {
        public Employee()
        {
            EmployeeLogs = new HashSet<EmployeeLog>();
            OvertimeTickets = new HashSet<OvertimeTicket>();
        }

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeHashCode { get; set; }
        public string EmployeeAddress { get; set; }

        public virtual ICollection<EmployeeLog> EmployeeLogs { get; set; }
        public virtual ICollection<OvertimeTicket> OvertimeTickets { get; set; }
    }
}
