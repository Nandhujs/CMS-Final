using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Models
{
    public class PatientDetailViewModel
    {
        // Patient basic info
        public int PatientId { get; set; }
        public string MMRNo { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public int? Age { get; set; }
        public string BloodGroup { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }

        // Lists from the other result sets
        public List<DiagnosisHistoryViewModel> Consultations { get; set; } = new();
        public List<LabResultViewModel> LabResults { get; set; } = new();
    }
}