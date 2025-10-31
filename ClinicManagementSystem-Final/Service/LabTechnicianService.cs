using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Repository;

namespace ClinicManagementSystem_Final.Service
{
    public class LabTechnicianService : ILabTechnicianService
    {
        private readonly ILabTechnicianRepository _labTechnicianRepository;

        public LabTechnicianService(ILabTechnicianRepository labTechnicianRepository)
        {
            _labTechnicianRepository = labTechnicianRepository;
        }

        public async Task<LabPatient> SearchPatientAsync(string searchTerm, string searchType)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return null;

            if (searchType == "MMR")
            {
                return await _labTechnicianRepository.GetPatientByMMRAsync(searchTerm);
            }
            else // Search by Name
            {
                var patients = await _labTechnicianRepository.SearchPatientsByNameAsync(searchTerm);
                return patients.Count > 0 ? patients[0] : null;
            }
        }

        public async Task<List<LabResultViewModel>> GetPatientPendingTestsAsync(int patientId)
        {
            return await _labTechnicianRepository.GetPendingTestsByPatientAsync(patientId);
        }

        public async Task<List<LabResultViewModel>> GetAllPendingTestsAsync()
        {
            return await _labTechnicianRepository.GetAllPendingTestsAsync();
        }

        public async Task<LabResultViewModel> GetLabTestForUpdateAsync(int testId, int patientId)
        {
            return await _labTechnicianRepository.GetLabTestDetailsAsync(testId, patientId);
        }

        public async Task<List<LabTest>> GetAllAvailableTestsAsync()
        {
            return await _labTechnicianRepository.GetAllLabTestsAsync();
        }

        public async Task<(bool success, int resultId, string status, string statusColor)> UpdateLabResultAsync(LabResultViewModel labResult, int technicianId)
        {
            return await _labTechnicianRepository.UpdateLabResultAsync(labResult, technicianId);
        }

        public async Task<LabReportViewModel> GenerateLabReportAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _labTechnicianRepository.GenerateLabReportAsync(patientId, fromDate, toDate);
        }
    }
}

