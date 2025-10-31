namespace ClinicManagementSystem_Final.Models
{
    public class WaitlistItem
    {
        public int WaitlistId { get; set; }
        public int PatientId { get; set; }
        public DateTime AddedDate { get; set; }
        public string Status { get; set; }  // e.g. "Waiting", "Called", "Removed"
    }
}
