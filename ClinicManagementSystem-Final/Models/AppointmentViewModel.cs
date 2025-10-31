namespace ClinicManagementSystem_Final.Models
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } // "Scheduled", "Completed", "Missed"
        public int? TokenNumber { get; set; }
        public string Gender { get; set; }
        public string MMRNo { get; set; }
        public int? Age { get; set; }
        public string Specialization { get; set; }
        public decimal? ConsultationFee { get; set; }
    }
}
