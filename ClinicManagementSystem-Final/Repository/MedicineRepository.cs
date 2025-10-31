using ClinicManagementSystem_Final.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data;
using Dapper;

namespace ClinicManagementSystem_Final.Repository
{
    public class MedicineRepository : IMedicineRepository
    {
        private readonly string _connectionString;

        public MedicineRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MVCConnectionString");
        }

        public async Task<IEnumerable<Medicine>> GetAllMedicinesAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Using inline SQL since we don't have a specific procedure for this
                var medicines = await connection.QueryAsync<Medicine>(
                    "SELECT MedicineId, MedicineName, MedicineDescription, Quantity, Price FROM Medicine ORDER BY MedicineName");
                return medicines;
            }
        }

        public async Task<Medicine> GetMedicineByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var medicine = await connection.QueryFirstOrDefaultAsync<Medicine>(
                    "SELECT MedicineId, MedicineName, MedicineDescription, Quantity, Price FROM Medicine WHERE MedicineId = @MedicineId",
                    new { MedicineId = id });
                return medicine;
            }
        }

        public async Task<int> AddMedicineAsync(Medicine medicine)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var id = await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO Medicine (MedicineName, MedicineDescription, Quantity, Price) 
                      OUTPUT INSERTED.MedicineId 
                      VALUES (@MedicineName, @MedicineDescription, @Quantity, @Price)",
                    new
                    {
                        medicine.MedicineName,
                        medicine.MedicineDescription,
                        medicine.Quantity,
                        medicine.Price
                    });
                return id;
            }
        }

        public async Task<bool> UpdateMedicineAsync(Medicine medicine)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var rowsAffected = await connection.ExecuteAsync(
                    @"UPDATE Medicine SET MedicineName = @MedicineName, MedicineDescription = @MedicineDescription, 
                      Quantity = @Quantity, Price = @Price 
                      WHERE MedicineId = @MedicineId",
                    new
                    {
                        medicine.MedicineId,
                        medicine.MedicineName,
                        medicine.MedicineDescription,
                        medicine.Quantity,
                        medicine.Price
                    });
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteMedicineAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var rowsAffected = await connection.ExecuteAsync(
                    "DELETE FROM Medicine WHERE MedicineId = @MedicineId",
                    new { MedicineId = id });
                return rowsAffected > 0;
            }
        }

        public async Task<IEnumerable<Medicine>> SearchMedicinesAsync(string searchTerm)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var medicines = await connection.QueryAsync<Medicine>(
                    @"SELECT MedicineId, MedicineName, MedicineDescription, Quantity, Price 
                      FROM Medicine 
                      WHERE MedicineName LIKE @SearchTerm OR MedicineDescription LIKE @SearchTerm 
                      ORDER BY MedicineName",
                    new { SearchTerm = $"%{searchTerm}%" });
                return medicines;
            }
        }

        public async Task<IEnumerable<Medicine>> GetLowStockMedicinesAsync(int threshold = 10)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var medicines = await connection.QueryAsync<Medicine>(
                    @"SELECT MedicineId, MedicineName, MedicineDescription, Quantity, Price 
                      FROM Medicine 
                      WHERE Quantity <= @Threshold 
                      ORDER BY Quantity",
                    new { Threshold = threshold });
                return medicines;
            }
        }

        public async Task<bool> UpdateStockAsync(int medicineId, int quantity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var rowsAffected = await connection.ExecuteAsync(
                    "UPDATE Medicine SET Quantity = @Quantity WHERE MedicineId = @MedicineId",
                    new { MedicineId = medicineId, Quantity = quantity });
                return rowsAffected > 0;
            }
        }
    }
}

