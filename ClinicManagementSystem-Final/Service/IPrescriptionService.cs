using ClinicManagementSystem_Final.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicManagementSystem_Final.Service
{
    public interface IPrescriptionService
    {
        Task<IEnumerable<Prescription>> GetAllPrescriptionsAsync();
        Task<Prescription> GetPrescriptionByIdAsync(int prescriptionId);
        Task<IEnumerable<Prescription>> GetPendingPrescriptionsAsync();
        Task<bool> DispensePrescriptionAsync(int prescriptionId);
        Task<PrescriptionViewModel> GetPrescriptionForBillAsync(int prescriptionId);
        Task<bool> IsPrescriptionDispensed(int prescriptionId);
    }
}



