using ClinicManagementSystem_Final.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicManagementSystem_Final.Repository
{
    public interface IPrescriptionRepository
    {
        Task<IEnumerable<Prescription>> GetAllPrescriptionsAsync();
        Task<Prescription> GetPrescriptionByIdAsync(int prescriptionId);
        Task<IEnumerable<Prescription>> GetPendingPrescriptionsAsync();
        Task<bool> DispensePrescriptionAsync(int prescriptionId);
        Task<Prescription> GetPrescriptionDetailsForBillAsync(int prescriptionId);
        Task<bool> IsPrescriptionDispensed(int prescriptionId);
    }
}


