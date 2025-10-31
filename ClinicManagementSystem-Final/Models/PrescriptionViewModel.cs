using System;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Models
{
    public class PrescriptionViewModel
    {
        public int PrescriptionId { get; set; }
        public int AppointmentId { get; set; }
        public string MMRNo { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Diagnosis { get; set; }
        public DateTime ConsultationDate { get; set; }
        public List<PrescribedMedicineViewModel> Medicines { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PrescribedMedicineViewModel
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public string Dosage { get; set; }
        public string Duration { get; set; }
        public string Type { get; set; }
        public string Frequency { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal => Quantity * Price;
    }
}


