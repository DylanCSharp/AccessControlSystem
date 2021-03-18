#nullable disable

namespace AccessControlSystem.Models
{
    public partial class EmployeeLog
    {
        public int EmployeeLogNumber { get; set; }
        public int? EmployeeId { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public string DateLog { get; set; }
        public int? CheckInStatus { get; set; }

        public virtual Employee Employee { get; set; }
    }
}
