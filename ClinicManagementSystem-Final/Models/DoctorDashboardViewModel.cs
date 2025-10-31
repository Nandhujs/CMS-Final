namespace ClinicManagementSystem_Final.Models
{
    public class DoctorDashboardViewModel
    {
        public int AppointmentId { get; set; }
        public int TokenNumber { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public string MMRNo { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public int? Age { get; set; }
    }
}
