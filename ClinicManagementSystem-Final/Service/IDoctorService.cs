using ClinicManagementSystem_Final.Models;
using System;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Service
{
    public interface IDoctorService
    {
        public List<DoctorDashboardViewModel> GetAppointments(int doctorId, DateTime forDate);
        public int? GetDoctorIdByStaffId(int staffId);
        public PatientDetailViewModel GetPatientByMMR(string mmrNo);
        public List<MedicineViewModel> GetAvailableMedicines();
        public List<LabTestViewModel> GetAllLabTests();

        public ConsultationViewModel GetConsultationByAppointment(int appointmentId);
        public int SaveConsultation(int appointmentId, int patientId, int doctorId,
            string symptoms, string diagnosis, string prescribedMedicine, string prescribedLab, string doctorNotes);
        public void UpdateAppointmentStatus(int appointmentId, string newStatus);
        public List<LabResultViewModel> GetLabResultsForPatient(int patientId, DateTime? fromDate = null, DateTime? toDate = null);
        Doctor GetDoctorByStaffId(int staffId);

    }
}
