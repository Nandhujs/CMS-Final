namespace ClinicManagementSystem_Final.Models
{
    public class ConsultationCreateViewModel
    {
        public int PatientId { get; set; }
        public string MMRNo { get; set; }
        public int AppointmentId { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public string PrescribedMedicine { get; set; }
        public string PrescribedLab { get; set; }
        public string DoctorNotes { get; set; }
    }
}