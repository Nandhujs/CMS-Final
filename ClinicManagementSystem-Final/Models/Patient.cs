using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem_Final.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        
        [Required(ErrorMessage = "Patient name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? PatientName { get; set; }
        
        [Required(ErrorMessage = "Gender is required")]
        public string? Gender { get; set; }
        
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DOB { get; set; }
        
        [Required(ErrorMessage = "Contact number is required")]
        [StringLength(15, ErrorMessage = "Contact number cannot exceed 15 characters")]
        public string? ContactNumber { get; set; }
        
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }
        
        public string? MMRNo { get; set; }
        public int DoctorId { get; set; }
        
        [StringLength(10, ErrorMessage = "Blood group cannot exceed 10 characters")]
        public string? BloodGroup { get; set; }
        
        public string? Status { get; set; } = "Active"; // default value
    }
}
