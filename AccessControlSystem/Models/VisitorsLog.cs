#nullable disable

namespace AccessControlSystem.Models
{
    public partial class VisitorsLog
    {
        public int VisitLogNumber { get; set; }
        public string VisitorName { get; set; }
        public string VisitorReason { get; set; }
        public string VisitorSeeingWhom { get; set; }
        public string VisitorTimestamp { get; set; }
        public string VisitorPictureUrl { get; set; }
    }
}
