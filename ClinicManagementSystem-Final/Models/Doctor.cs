namespace ClinicManagementSystem_Final.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public int StaffId { get; set; }
        public string DoctorName { get; set; }
        public string SpecializationName { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }

        // Foreign Key to Specialization table
        public int SpecializationId { get; set; }
        public Specialization Specialization { get; set; }

        public decimal? Fee { get; set; }  // Add this if your db supports it
    }
}
