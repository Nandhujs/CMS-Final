using ClinicManagementSystem_Final.Models;
using System;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Repository
{
    public interface IDoctorRepository
    {
        List<DoctorDashboardViewModel> GetAppointments(int doctorId, DateTime forDate);
        int? GetDoctorIdByStaffId(int staffId);
        PatientDetailViewModel GetPatientByMMR(string mmrNo);

        List<MedicineViewModel> GetAvailableMedicines();
        List<LabTestViewModel> GetAllLabTests();

        ConsultationViewModel GetConsultationByAppointment(int appointmentId);
        int SaveConsultation(int appointmentId, int patientId, int doctorId,
            string symptoms, string diagnosis, string prescribedMedicine, string prescribedLab, string doctorNotes);
        void UpdateAppointmentStatus(int appointmentId, string newStatus);
        List<LabResultViewModel> GetLabResultsForPatient(int patientId, DateTime? fromDate = null, DateTime? toDate = null);
        Doctor GetDoctorByStaffId(int staffId);

    }
}
