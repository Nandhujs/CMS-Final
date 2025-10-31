using System;

namespace ClinicManagementSystem_Final.Models
{
    public class AppointmentBillViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; }
        public string MMRNo { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public int TokenNumber { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public decimal ConsultationFee { get; set; }
        public DateTime BillDate { get; set; }
    }
}

