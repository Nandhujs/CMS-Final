using ClinicManagementSystem_Final.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicManagementSystem_Final.Repository
{
    public interface IMedicineRepository
    {
        Task<IEnumerable<Medicine>> GetAllMedicinesAsync();
        Task<Medicine> GetMedicineByIdAsync(int id);
        Task<int> AddMedicineAsync(Medicine medicine);
        Task<bool> UpdateMedicineAsync(Medicine medicine);
        Task<bool> DeleteMedicineAsync(int id);
        Task<IEnumerable<Medicine>> SearchMedicinesAsync(string searchTerm);
        Task<IEnumerable<Medicine>> GetLowStockMedicinesAsync(int threshold = 10);
        Task<bool> UpdateStockAsync(int medicineId, int quantity);
    }
}


