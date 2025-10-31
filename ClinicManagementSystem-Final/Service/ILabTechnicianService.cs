using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicManagementSystem_Final.Models;

namespace ClinicManagementSystem_Final.Service
{
    public interface ILabTechnicianService
    {
        Task<LabPatient> SearchPatientAsync(string searchTerm, string searchType);
        Task<List<LabResultViewModel>> GetPatientPendingTestsAsync(int patientId);
        Task<List<LabResultViewModel>> GetAllPendingTestsAsync(); // Get all pending tests for all patients
        Task<LabResultViewModel> GetLabTestForUpdateAsync(int testId, int patientId);
        Task<List<LabTest>> GetAllAvailableTestsAsync();
        Task<(bool success, int resultId, string status, string statusColor)> UpdateLabResultAsync(LabResultViewModel labResult, int technicianId);
        Task<LabReportViewModel> GenerateLabReportAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}

