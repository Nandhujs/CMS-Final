using ClinicManagementSystem_Final.Models;
using System;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Service
{
    public interface IReceptionistService
    {
        // Patient Management
        List<Patient> SearchPatientsByPhone(string phone);
        List<Patient> SearchPatientsByMMRNo(string mmrNo);
        void AddPatient(Patient patient);
        Patient GetPatientById(int id);
        void UpdatePatient(Patient patient);
        void DeletePatient(int id); // <-- Add this

        // Scheduling / Appointments
        List<Doctor> GetAllDoctors();
        List<Patient> GetAllPatients();
        List<AppointmentViewModel> GetTodayAppointments();
        (int appointmentId, int tokenNumber) BookAppointment(int patientId, int doctorId, DateTime appointmentDate);
        AppointmentBillViewModel GetAppointmentBill(int appointmentId);
    }
}
