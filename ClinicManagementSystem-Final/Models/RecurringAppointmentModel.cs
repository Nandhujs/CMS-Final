namespace ClinicManagementSystem_Final.Models
{
    public class RecurringAppointmentModel
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Frequency { get; set; } // e.g., "Weekly", "Monthly"
        public string Notes { get; set; }
    }
}
