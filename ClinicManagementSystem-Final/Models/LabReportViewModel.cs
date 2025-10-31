using System;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Models
{
    public class LabReportViewModel
    {
        public PatientInfo Patient { get; set; }
        public List<LabTestResult> TestResults { get; set; }
        public DateTime ReportDate { get; set; }
        public string GeneratedBy { get; set; }
    }

    public class PatientInfo
    {
        public int PatientId { get; set; }
        public string MMRNo { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public int? Age { get; set; }
        public string BloodGroup { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class LabTestResult
    {
        public string TestName { get; set; }
        public string NormalRange { get; set; }
        public string SampleType { get; set; }
        public decimal? ActualValue { get; set; }
        public decimal? LowRange { get; set; }
        public decimal? HighRange { get; set; }
        public string Remarks { get; set; }
        public string DoctorReview { get; set; }
        public DateTime Date { get; set; }
        public string TechnicianName { get; set; }
        public string ValueStatus { get; set; }
        public string StatusColor { get; set; }
    }
}


