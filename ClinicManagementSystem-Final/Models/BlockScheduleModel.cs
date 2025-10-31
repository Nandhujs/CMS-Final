namespace ClinicManagementSystem_Final.Models
{
    public class BlockScheduleModel
    {
        public int DoctorId { get; set; }
        public DateTime BlockStartDate { get; set; }
        public DateTime BlockEndDate { get; set; }
        public string Reason { get; set; }
    }
}
