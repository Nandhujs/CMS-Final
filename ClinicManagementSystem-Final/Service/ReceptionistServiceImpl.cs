using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Repository;
using System;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Service
{
    public class ReceptionistServiceImpl : IReceptionistService
    {
        private readonly IReceptionistRepository _receptionistRepository;

        public ReceptionistServiceImpl(IReceptionistRepository receptionistRepository)
        {
            _receptionistRepository = receptionistRepository;
        }

        // ---------------------- Patient Management ----------------------
        public void AddPatient(Patient patient)
        {
            _receptionistRepository.AddPatient(patient);
        }

        public Patient GetPatientById(int id)
        {
            return _receptionistRepository.GetPatientById(id);
        }

        public void UpdatePatient(Patient patient)
        {
            _receptionistRepository.UpdatePatient(patient);
        }

        public void DeletePatient(int id)
        {
            _receptionistRepository.DeletePatient(id);
        }

        public List<Patient> SearchPatientsByMMRNo(string mmrNo)
        {
            return _receptionistRepository.SearchPatientsByMMRNo(mmrNo);
        }

        public List<Patient> SearchPatientsByPhone(string phone)
        {
            return _receptionistRepository.SearchPatientsByPhone(phone);
        }

        // ---------------------- Scheduling / Appointments ----------------------
        public List<Doctor> GetAllDoctors()
        {
            return _receptionistRepository.GetAllDoctors();
        }

        public List<Patient> GetAllPatients()
        {
            return _receptionistRepository.GetAllPatients();
        }

        public List<AppointmentViewModel> GetTodayAppointments()
        {
            return _receptionistRepository.GetTodayAppointments();
        }

        public (int appointmentId, int tokenNumber) BookAppointment(int patientId, int doctorId, DateTime appointmentDate)
        {
            return _receptionistRepository.BookAppointment(patientId, doctorId, appointmentDate);
        }

        public AppointmentBillViewModel GetAppointmentBill(int appointmentId)
        {
            return _receptionistRepository.GetAppointmentBill(appointmentId);
        }
    }
}
