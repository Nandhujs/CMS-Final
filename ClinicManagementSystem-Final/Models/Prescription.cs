using System;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Models
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; } // Added DoctorId property
        public string MMRNo { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Diagnosis { get; set; }
        public DateTime ConsultationDate { get; set; }
        public string Status { get; set; }

        // 👇 Add this property
        public string PrescribedMedicine { get; set; }

        // 👇 And this list to hold parsed medicine details
        public List<PrescribedMedicine> Medicines { get; set; }
    }

    public class PrescribedMedicine
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public string Dosage { get; set; }
        public string Duration { get; set; }
        public string Type { get; set; }
        public string Frequency { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}


