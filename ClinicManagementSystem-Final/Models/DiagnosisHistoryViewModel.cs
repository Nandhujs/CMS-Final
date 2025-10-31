namespace ClinicManagementSystem_Final.Models
{
    public class DiagnosisHistoryViewModel
    {
        public int DiagnosisId { get; set; }
        public int AppointmentId { get; set; }
        public DateTime? Date { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public string PrescribedMedicine { get; set; }
        public string PrescribedLab { get; set; }
        public string DoctorNotes { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public int? WhoDoctorId { get; set; }
        public string WhoDoctorName { get; set; }
    }
}