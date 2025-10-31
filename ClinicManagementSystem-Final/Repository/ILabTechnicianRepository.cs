using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicManagementSystem_Final.Models;

namespace ClinicManagementSystem_Final.Repository
{
    public interface ILabTechnicianRepository
    {
        Task<LabPatient> GetPatientByIdAsync(int patientId);
        Task<LabPatient> GetPatientByMMRAsync(string mmrNo);
        Task<List<LabPatient>> SearchPatientsByNameAsync(string name);
        Task<List<LabResultViewModel>> GetPendingTestsByPatientAsync(int patientId);
        Task<List<LabResultViewModel>> GetAllPendingTestsAsync(); // Get all pending tests for all patients
        Task<List<LabTest>> GetAllLabTestsAsync();
        Task<LabResultViewModel> GetLabTestDetailsAsync(int testId, int patientId);
        Task<bool> AddOrUpdateLabResultAsync(LabResultViewModel labResult);
        Task<LabTest> GetLabTestByIdAsync(int testId);

        // ADD THESE MISSING METHODS:
        Task<(bool success, int resultId, string status, string statusColor)> UpdateLabResultAsync(LabResultViewModel labResult, int technicianId);
        Task<LabReportViewModel> GenerateLabReportAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}

