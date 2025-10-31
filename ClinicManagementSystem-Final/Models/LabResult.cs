using System;

namespace ClinicManagementSystem_Final.Models
{
    public class LabResult
    {
        public int ResultId { get; set; }
        public int TestId { get; set; }
        public int PatientId { get; set; }
        public decimal? LowRange { get; set; }
        public decimal? HighRange { get; set; }
        public decimal? ActualValue { get; set; }
        public string Remarks { get; set; }
        public string DoctorReview { get; set; }
        public DateTime Date { get; set; }
        public string TestName { get; set; }
        public string PatientName { get; set; }
        public string Status { get; set; }
    }
}


