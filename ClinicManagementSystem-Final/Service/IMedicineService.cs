using ClinicManagementSystem_Final.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicManagementSystem_Final.Service
{
    public interface IMedicineService
    {
        Task<IEnumerable<Medicine>> GetAllMedicinesAsync();
        Task<Medicine> GetMedicineByIdAsync(int id);
        Task<bool> AddMedicineAsync(MedicineViewModel medicine);
        Task<bool> UpdateMedicineAsync(MedicineViewModel medicine);
        Task<bool> DeleteMedicineAsync(int id);
        Task<IEnumerable<Medicine>> SearchMedicinesAsync(string searchTerm);
        Task<IEnumerable<Medicine>> GetLowStockMedicinesAsync();
    }
}



