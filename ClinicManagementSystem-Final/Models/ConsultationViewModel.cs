using System;

namespace ClinicManagementSystem_Final.Models
{
    public class ConsultationViewModel
    {
        // Appointment / patient info
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int? DoctorId { get; set; }
        public int TokenNumber { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public string MMRNo { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public int? Age { get; set; }

        // Existing diagnosis for this appointment (if any)
        public DiagnosisHistoryViewModel ExistingDiagnosis { get; set; }
    }
}