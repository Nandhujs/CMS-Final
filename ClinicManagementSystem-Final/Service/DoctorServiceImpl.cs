using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Repository;
using System;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Service
{
    public class DoctorServiceImpl : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;

        public DoctorServiceImpl(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public List<DoctorDashboardViewModel> GetAppointments(int doctorId, DateTime forDate)
        {
            return _doctorRepository.GetAppointments(doctorId, forDate);
        }

        public int? GetDoctorIdByStaffId(int staffId)
        {
            return _doctorRepository.GetDoctorIdByStaffId(staffId);
        }

        public PatientDetailViewModel GetPatientByMMR(string mmrNo)
        {
            return _doctorRepository.GetPatientByMMR(mmrNo);
        }

        public List<MedicineViewModel> GetAvailableMedicines()
        {
            return _doctorRepository.GetAvailableMedicines();
        }

        public List<LabTestViewModel> GetAllLabTests()
        {
            return _doctorRepository.GetAllLabTests();
        }

        public ConsultationViewModel GetConsultationByAppointment(int appointmentId)
        {
            return _doctorRepository.GetConsultationByAppointment(appointmentId);
        }

        public int SaveConsultation(int appointmentId, int patientId, int doctorId, string symptoms, string diagnosis, string prescribedMedicine, string prescribedLab, string doctorNotes)
        {
            return _doctorRepository.SaveConsultation(appointmentId, patientId, doctorId, symptoms, diagnosis, prescribedMedicine, prescribedLab, doctorNotes);
        }

        public void UpdateAppointmentStatus(int appointmentId, string newStatus)
        {
            _doctorRepository.UpdateAppointmentStatus(appointmentId, newStatus);
        }

        public List<LabResultViewModel> GetLabResultsForPatient(int patientId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return _doctorRepository.GetLabResultsForPatient(patientId, fromDate, toDate);
        }

        public Doctor GetDoctorByStaffId(int staffId)
        {
            return _doctorRepository.GetDoctorByStaffId(staffId);
        }
    }
}
