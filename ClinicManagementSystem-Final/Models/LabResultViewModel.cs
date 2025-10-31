using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem_Final.Models
{
    public class LabResultViewModel
    {
        public int ResultId { get; set; }

        [Required]
        public int TestId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Display(Name = "Low Range")]
        public decimal? LowRange { get; set; }

        [Display(Name = "High Range")]
        public decimal? HighRange { get; set; }

        [Required(ErrorMessage = "Actual Value is required")]
        [Display(Name = "Actual Value")]
        public decimal? ActualValue { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Doctor Review")]
        public string? DoctorReview { get; set; }

        public DateTime Date { get; set; }

        [Display(Name = "Test Name")]
        public string TestName { get; set; }

        [Display(Name = "Patient Name")]
        public string PatientName { get; set; }

        public string? MMRNo { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? TestStatus { get; set; }
        public string? NormalRange { get; set; }
        public string? SampleType { get; set; }

        // Only set after results are entered
        public string? ValueStatus { get; set; }
        public string? StatusColor { get; set; }
    }
}
