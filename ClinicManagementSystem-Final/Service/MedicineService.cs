using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicManagementSystem_Final.Service
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository _medicineRepository;

        public MedicineService(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<IEnumerable<Medicine>> GetAllMedicinesAsync()
        {
            return await _medicineRepository.GetAllMedicinesAsync();
        }

        public async Task<Medicine> GetMedicineByIdAsync(int id)
        {
            return await _medicineRepository.GetMedicineByIdAsync(id);
        }

        public async Task<bool> AddMedicineAsync(MedicineViewModel medicineViewModel)
        {
            var medicine = new Medicine
            {
                MedicineName = medicineViewModel.MedicineName,
                MedicineDescription = medicineViewModel.MedicineDescription,
                Quantity = medicineViewModel.Quantity,
                Price = medicineViewModel.Price
            };

            var id = await _medicineRepository.AddMedicineAsync(medicine);
            return id > 0;
        }

        public async Task<bool> UpdateMedicineAsync(MedicineViewModel medicineViewModel)
        {
            var medicine = new Medicine
            {
                MedicineId = medicineViewModel.MedicineId,
                MedicineName = medicineViewModel.MedicineName,
                MedicineDescription = medicineViewModel.MedicineDescription,
                Quantity = medicineViewModel.Quantity,
                Price = medicineViewModel.Price
            };

            return await _medicineRepository.UpdateMedicineAsync(medicine);
        }

        public async Task<bool> DeleteMedicineAsync(int id)
        {
            return await _medicineRepository.DeleteMedicineAsync(id);
        }

        public async Task<IEnumerable<Medicine>> SearchMedicinesAsync(string searchTerm)
        {
            return await _medicineRepository.SearchMedicinesAsync(searchTerm);
        }

        public async Task<IEnumerable<Medicine>> GetLowStockMedicinesAsync()
        {
            return await _medicineRepository.GetLowStockMedicinesAsync(10);
        }
    }
}

