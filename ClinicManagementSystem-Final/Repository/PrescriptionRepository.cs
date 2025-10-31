using ClinicManagementSystem_Final.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data;
using System.Linq;
using Dapper;

namespace ClinicManagementSystem_Final.Repository
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly string _connectionString;

        public PrescriptionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MVCConnectionString");
        }

        public async Task<IEnumerable<Prescription>> GetAllPrescriptionsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT DISTINCT
                        d.DiagnosisId as PrescriptionId,
                        a.AppointmentId,
                        p.PatientId,
                        d.DoctorId,
                        p.MMRNo,
                        p.Name as PatientName,
                        s.Name as DoctorName,
                        d.Diagnosis,
                        a.AppointmentDate as ConsultationDate,
                        CASE 
                            WHEN EXISTS (SELECT 1 FROM IssuedMedicine im WHERE im.AppointmentId = a.AppointmentId) 
                            THEN 'Dispensed' 
                            ELSE 'Pending' 
                        END as Status
                    FROM DiagnosisDetails d
                    INNER JOIN Appointment a ON d.AppointmentId = a.AppointmentId
                    INNER JOIN Patient p ON d.PatientId = p.PatientId
                    INNER JOIN Doctor doc ON d.DoctorId = doc.DoctorId
                    INNER JOIN Staff s ON doc.StaffId = s.StaffId
                    WHERE EXISTS (SELECT 1 FROM MedPrescription mp WHERE mp.AppointmentId = a.AppointmentId)
                    ORDER BY a.AppointmentDate DESC";

                var prescriptions = await connection.QueryAsync<Prescription>(sql);
                
                // Load medicines for each prescription
                foreach (var prescription in prescriptions)
                {
                    prescription.Medicines = await LoadPrescribedMedicinesAsync(prescription.AppointmentId);
                }
                
                return prescriptions;
            }
        }

        public async Task<Prescription> GetPrescriptionByIdAsync(int prescriptionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT DISTINCT
                        d.DiagnosisId as PrescriptionId,
                        a.AppointmentId,
                        p.PatientId,
                        d.DoctorId,
                        p.MMRNo,
                        p.Name as PatientName,
                        s.Name as DoctorName,
                        d.Diagnosis,
                        a.AppointmentDate as ConsultationDate,
                        CASE 
                            WHEN EXISTS (SELECT 1 FROM IssuedMedicine im WHERE im.AppointmentId = a.AppointmentId) 
                            THEN 'Dispensed' 
                            ELSE 'Pending' 
                        END as Status
                    FROM DiagnosisDetails d
                    INNER JOIN Appointment a ON d.AppointmentId = a.AppointmentId
                    INNER JOIN Patient p ON d.PatientId = p.PatientId
                    INNER JOIN Doctor doc ON d.DoctorId = doc.DoctorId
                    INNER JOIN Staff s ON doc.StaffId = s.StaffId
                    WHERE d.DiagnosisId = @PrescriptionId";

                var prescription = await connection.QueryFirstOrDefaultAsync<Prescription>(sql, new { PrescriptionId = prescriptionId });
                
                if (prescription != null)
                {
                    prescription.Medicines = await LoadPrescribedMedicinesAsync(prescription.AppointmentId);
                }

                return prescription;
            }
        }

        public async Task<IEnumerable<Prescription>> GetPendingPrescriptionsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT DISTINCT
                        d.DiagnosisId as PrescriptionId,
                        a.AppointmentId,
                        p.PatientId,
                        d.DoctorId,
                        p.MMRNo,
                        p.Name as PatientName,
                        s.Name as DoctorName,
                        d.Diagnosis,
                        a.AppointmentDate as ConsultationDate,
                        'Pending' as Status
                    FROM DiagnosisDetails d
                    INNER JOIN Appointment a ON d.AppointmentId = a.AppointmentId
                    INNER JOIN Patient p ON d.PatientId = p.PatientId
                    INNER JOIN Doctor doc ON d.DoctorId = doc.DoctorId
                    INNER JOIN Staff s ON doc.StaffId = s.StaffId
                    WHERE NOT EXISTS (SELECT 1 FROM IssuedMedicine im WHERE im.AppointmentId = a.AppointmentId)
                    AND EXISTS (SELECT 1 FROM MedPrescription mp WHERE mp.AppointmentId = a.AppointmentId)
                    ORDER BY a.AppointmentDate DESC";

                var prescriptions = await connection.QueryAsync<Prescription>(sql);
                
                // Load medicines for each prescription
                foreach (var prescription in prescriptions)
                {
                    prescription.Medicines = await LoadPrescribedMedicinesAsync(prescription.AppointmentId);
                }
                
                return prescriptions;
            }
        }

        public async Task<bool> DispensePrescriptionAsync(int prescriptionId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DispensePrescriptionAsync called with PrescriptionId: {prescriptionId}");
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Get prescription details
                            var prescription = await GetPrescriptionByIdAsync(prescriptionId);
                            if (prescription == null || prescription.Medicines == null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Prescription not found or no medicines for ID: {prescriptionId}");
                                return false;
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"Prescription found: AppointmentId={prescription.AppointmentId}, PatientId={prescription.PatientId}, DoctorId={prescription.DoctorId}");

                        // Process each medicine in the prescription
                        foreach (var medicine in prescription.Medicines)
                        {
                            // Check stock availability
                            var currentStock = await connection.ExecuteScalarAsync<int>(
                                "SELECT Quantity FROM Medicine WHERE MedicineId = @MedicineId",
                                new { MedicineId = medicine.MedicineId }, transaction);

                            if (currentStock < medicine.Quantity)
                            {
                                throw new System.Exception($"Insufficient stock for {medicine.MedicineName}. Available: {currentStock}, Required: {medicine.Quantity}");
                            }

                            // Update stock
                            await connection.ExecuteAsync(
                                "UPDATE Medicine SET Quantity = Quantity - @Quantity WHERE MedicineId = @MedicineId",
                                new { MedicineId = medicine.MedicineId, Quantity = medicine.Quantity }, transaction);

                            // Record issued medicine
                            await connection.ExecuteAsync(
                                @"INSERT INTO IssuedMedicine (AppointmentId, PatientId, DoctorId, MedicineId, QuantityIssued, Dosage, IssueDate)
                                  VALUES (@AppointmentId, @PatientId, @DoctorId, @MedicineId, @Quantity, @Dosage, GETDATE())",
                                new
                                {
                                    prescription.AppointmentId,
                                    prescription.PatientId,
                                    DoctorId = prescription.DoctorId, // Fixed: Use actual DoctorId from prescription
                                    medicine.MedicineId,
                                    Quantity = medicine.Quantity,
                                    Dosage = $"{medicine.Quantity} tablets - {medicine.Frequency}"
                                }, transaction);
                        }

                        // Update prescription status
                        await connection.ExecuteAsync(
                            "UPDATE MedPrescription SET Status = 'Dispensed' WHERE AppointmentId = @AppointmentId",
                            new { prescription.AppointmentId }, transaction);

                            transaction.Commit();
                            System.Diagnostics.Debug.WriteLine($"Prescription {prescriptionId} dispensed successfully");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in transaction for prescription {prescriptionId}: {ex.Message}");
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DispensePrescriptionAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<Prescription> GetPrescriptionDetailsForBillAsync(int prescriptionId)
        {
            var prescription = await GetPrescriptionByIdAsync(prescriptionId);
            return prescription;
        }

        public async Task<bool> IsPrescriptionDispensed(int prescriptionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT COUNT(*) 
                    FROM IssuedMedicine im
                    INNER JOIN DiagnosisDetails dd ON im.AppointmentId = dd.AppointmentId
                    WHERE dd.DiagnosisId = @PrescriptionId";

                var count = await connection.ExecuteScalarAsync<int>(sql, new { PrescriptionId = prescriptionId });
                return count > 0;
            }
        }

        private async Task<List<PrescribedMedicine>> LoadPrescribedMedicinesAsync(int appointmentId)
        {
            var medicines = new List<PrescribedMedicine>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT 
                        mp.MedicineId,
                        mp.Quantity,
                        mp.Frequency,
                        mp.DurationDays,
                        m.MedicineName,
                        m.Price
                    FROM MedPrescription mp
                    INNER JOIN Medicine m ON mp.MedicineId = m.MedicineId
                    WHERE mp.AppointmentId = @AppointmentId";

                var results = await connection.QueryAsync<dynamic>(sql, new { AppointmentId = appointmentId });

                foreach (var result in results)
                {
                    medicines.Add(new PrescribedMedicine
                    {
                        MedicineId = result.MedicineId,
                        MedicineName = result.MedicineName,
                        Quantity = result.Quantity,
                        Frequency = result.Frequency ?? "Once daily",
                        Duration = $"{result.DurationDays} days",
                        Dosage = $"{result.Quantity} tablets",
                        Type = "Tablet",
                        Price = result.Price
                    });
                }
            }

            return medicines;
        }
    }
}
