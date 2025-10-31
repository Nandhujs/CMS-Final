using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Diagnostics;
using ClinicManagementSystem_Final.Models;
using Microsoft.Extensions.Configuration;

namespace ClinicManagementSystem_Final.Repository
{
    public class LabTechnicianRepository : ILabTechnicianRepository
    {
        private readonly string _connectionString;

        public LabTechnicianRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MVCConnectionString");
        }

        public async Task<LabPatient> GetPatientByIdAsync(int patientId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"SELECT PatientId, MMRNo, Name, Gender, DOB, Phone, Status,
                                                     DATEDIFF(YEAR, DOB, GETDATE()) as Age 
                                                     FROM Patient WHERE PatientId = @PatientId", connection))
                {
                    command.Parameters.AddWithValue("@PatientId", patientId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new LabPatient
                            {
                                PatientId = reader.GetInt32("PatientId"),
                                MMRNo = reader.GetString("MMRNo"),
                                Name = reader.GetString("Name"),
                                Gender = reader.GetString("Gender"),
                                Age = reader.IsDBNull("Age") ? null : (int?)reader.GetInt32("Age"),
                                Phone = reader.GetString("Phone"),
                                Status = reader.GetString("Status")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<LabPatient> GetPatientByMMRAsync(string mmrNo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"SELECT PatientId, MMRNo, Name, Gender, DOB, Phone, Status,
                                                     DATEDIFF(YEAR, DOB, GETDATE()) as Age 
                                                     FROM Patient WHERE MMRNo = @MMRNo", connection))
                {
                    command.Parameters.AddWithValue("@MMRNo", mmrNo);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new LabPatient
                            {
                                PatientId = reader.GetInt32("PatientId"),
                                MMRNo = reader.GetString("MMRNo"),
                                Name = reader.GetString("Name"),
                                Gender = reader.GetString("Gender"),
                                Age = reader.IsDBNull("Age") ? null : (int?)reader.GetInt32("Age"),
                                Phone = reader.GetString("Phone"),
                                Status = reader.GetString("Status")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<List<LabPatient>> SearchPatientsByNameAsync(string name)
        {
            var patients = new List<LabPatient>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"SELECT PatientId, MMRNo, Name, Gender, DOB, Phone, Status,
                                                     DATEDIFF(YEAR, DOB, GETDATE()) as Age 
                                                     FROM Patient WHERE Name LIKE @Name AND Status = 'Active'", connection))
                {
                    command.Parameters.AddWithValue("@Name", $"%{name}%");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            patients.Add(new LabPatient
                            {
                                PatientId = reader.GetInt32("PatientId"),
                                MMRNo = reader.GetString("MMRNo"),
                                Name = reader.GetString("Name"),
                                Gender = reader.GetString("Gender"),
                                Age = reader.IsDBNull("Age") ? null : (int?)reader.GetInt32("Age"),
                                Phone = reader.GetString("Phone"),
                                Status = reader.GetString("Status")
                            });
                        }
                    }
                }
            }
            return patients;
        }

        public async Task<List<LabResultViewModel>> GetPendingTestsByPatientAsync(int patientId)
        {
            var tests = new List<LabResultViewModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
                    SELECT 
                        lr.ResultId,
                        lr.TestId,
                        lr.PatientId,
                        lr.LowRange,
                        lr.HighRange,
                        lr.ActualValue,
                        lr.Remarks,
                        lr.DoctorReview,
                        lr.Date,
                        lt.TestName,
                        p.Name as PatientName,
                        p.MMRNo,
                        p.Gender,
                        DATEDIFF(YEAR, p.DOB, GETDATE()) as Age,
                        lt.NormalRange,
                        lt.SampleType,
                        CASE WHEN lr.ActualValue IS NULL THEN 'Pending' ELSE 'Completed' END as TestStatus
                    FROM LabResult lr
                    INNER JOIN LabTest lt ON lr.TestId = lt.TestId
                    INNER JOIN Patient p ON lr.PatientId = p.PatientId
                    WHERE lr.PatientId = @PatientId", connection))
                {
                    command.Parameters.AddWithValue("@PatientId", patientId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tests.Add(new LabResultViewModel
                            {
                                ResultId = reader.GetInt32("ResultId"),
                                TestId = reader.GetInt32("TestId"),
                                PatientId = reader.GetInt32("PatientId"),
                                LowRange = reader.IsDBNull("LowRange") ? null : (decimal?)reader.GetDecimal("LowRange"),
                                HighRange = reader.IsDBNull("HighRange") ? null : (decimal?)reader.GetDecimal("HighRange"),
                                ActualValue = reader.IsDBNull("ActualValue") ? null : (decimal?)reader.GetDecimal("ActualValue"),
                                Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                                DoctorReview = reader.IsDBNull("DoctorReview") ? null : reader.GetString("DoctorReview"),
                                Date = reader.GetDateTime("Date"),
                                TestName = reader.GetString("TestName"),
                                PatientName = reader.GetString("PatientName"),
                                MMRNo = reader.GetString("MMRNo"),
                                Gender = reader.GetString("Gender"),
                                Age = reader.IsDBNull("Age") ? null : (int?)reader.GetInt32("Age"),
                                NormalRange = reader.GetString("NormalRange"),
                                SampleType = reader.GetString("SampleType"),
                                TestStatus = reader.GetString("TestStatus")
                                // ValueStatus and StatusColor are NOT set here - only after results are entered
                            });
                        }
                    }
                }
            }
            return tests;
        }

        public async Task<List<LabResultViewModel>> GetAllPendingTestsAsync()
        {
            var tests = new List<LabResultViewModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
                    SELECT 
                        ltp.PrescriptionId as ResultId,
                        ltp.TestId,
                        ltp.PatientId,
                        lr.LowRange,
                        lr.HighRange,
                        lr.ActualValue,
                        lr.Remarks,
                        lr.DoctorReview,
                        ltp.PrescribedDate as Date,
                        ltp.TestName,
                        p.Name as PatientName,
                        p.MMRNo,
                        p.Gender,
                        DATEDIFF(YEAR, p.DOB, GETDATE()) as Age,
                        lt.NormalRange,
                        lt.SampleType,
                        ltp.Status as TestStatus
                    FROM LabTestPrescription ltp
                    INNER JOIN LabTest lt ON ltp.TestId = lt.TestId
                    INNER JOIN Patient p ON ltp.PatientId = p.PatientId
                    LEFT JOIN LabResult lr ON lr.TestId = ltp.TestId AND lr.PatientId = ltp.PatientId
                    WHERE ltp.Status = 'Pending'
                    ORDER BY ltp.PrescribedDate DESC, p.Name", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tests.Add(new LabResultViewModel
                            {
                                ResultId = reader.GetInt32("ResultId"),
                                TestId = reader.GetInt32("TestId"),
                                PatientId = reader.GetInt32("PatientId"),
                                LowRange = reader.IsDBNull("LowRange") ? null : (decimal?)reader.GetDecimal("LowRange"),
                                HighRange = reader.IsDBNull("HighRange") ? null : (decimal?)reader.GetDecimal("HighRange"),
                                ActualValue = reader.IsDBNull("ActualValue") ? null : (decimal?)reader.GetDecimal("ActualValue"),
                                Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                                DoctorReview = reader.IsDBNull("DoctorReview") ? null : reader.GetString("DoctorReview"),
                                Date = reader.GetDateTime("Date"),
                                TestName = reader.GetString("TestName"),
                                PatientName = reader.GetString("PatientName"),
                                MMRNo = reader.GetString("MMRNo"),
                                Gender = reader.GetString("Gender"),
                                Age = reader.IsDBNull("Age") ? null : (int?)reader.GetInt32("Age"),
                                NormalRange = reader.IsDBNull("NormalRange") ? null : reader.GetString("NormalRange"),
                                SampleType = reader.IsDBNull("SampleType") ? null : reader.GetString("SampleType"),
                                TestStatus = reader.GetString("TestStatus")
                            });
                        }
                    }
                }
            }
            return tests;
        }

        public async Task<(bool success, int resultId, string status, string statusColor)> UpdateLabResultAsync(LabResultViewModel labResult, int technicianId)
        {
            Debug.WriteLine($"UpdateLabResultAsync called - TestId: {labResult.TestId}, PatientId: {labResult.PatientId}, ActualValue: {labResult.ActualValue}, Remarks: {labResult.Remarks}");
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                Debug.WriteLine($"Connected to database: {connection.Database}");
                
                // Simply insert or update - no complex logic
                using (var command = new SqlCommand(@"
                    IF NOT EXISTS (SELECT 1 FROM LabResult WHERE TestId = @TestId AND PatientId = @PatientId)
                    BEGIN
                        INSERT INTO LabResult (TestId, PatientId, ActualValue, Remarks, Date)
                        VALUES (@TestId, @PatientId, @ActualValue, @Remarks, GETDATE())
                        SELECT @@IDENTITY as ResultId
                    END
                    ELSE
                    BEGIN
                        UPDATE LabResult 
                        SET ActualValue = @ActualValue, Remarks = @Remarks
                        WHERE TestId = @TestId AND PatientId = @PatientId
                        SELECT ResultId FROM LabResult WHERE TestId = @TestId AND PatientId = @PatientId
                    END", connection))
                {
                    command.Parameters.AddWithValue("@TestId", labResult.TestId);
                    command.Parameters.AddWithValue("@PatientId", labResult.PatientId);
                    command.Parameters.AddWithValue("@ActualValue", labResult.ActualValue ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Remarks", labResult.Remarks ?? (object)DBNull.Value);

                    Debug.WriteLine($"Executing SQL with TestId={labResult.TestId}, PatientId={labResult.PatientId}");
                    
                    var resultIdObj = await command.ExecuteScalarAsync();
                    int resultId = resultIdObj != null ? Convert.ToInt32(resultIdObj) : 0;
                    
                    Debug.WriteLine($"SQL executed. ResultId: {resultId}");
                    
                    if (resultId > 0)
                    {
                        // Update LabTestPrescription status to Completed
                        using (var updateCmd = new SqlCommand(@"
                            UPDATE LabTestPrescription 
                            SET Status = 'Completed'
                            WHERE TestId = @TestId AND PatientId = @PatientId AND Status = 'Pending'", connection))
                        {
                            updateCmd.Parameters.AddWithValue("@TestId", labResult.TestId);
                            updateCmd.Parameters.AddWithValue("@PatientId", labResult.PatientId);
                            await updateCmd.ExecuteNonQueryAsync();
                            Debug.WriteLine("LabTestPrescription status updated to Completed");
                        }
                        
                        // Return success with normal status (no range checking for now)
                        return (success: true, resultId: resultId, status: "Value Saved", statusColor: "green");
                    }
                }
            }
            
            Debug.WriteLine("Update failed - no rows affected");
            return (success: false, resultId: 0, status: "", statusColor: "");
        }

        public async Task<LabReportViewModel> GenerateLabReportAsync(int patientId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var report = new LabReportViewModel
            {
                TestResults = new List<LabTestResult>(),
                ReportDate = DateTime.Now
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Get patient information
                using (var patientCommand = new SqlCommand(@"
                    SELECT PatientId, MMRNo, Name as PatientName, Gender, DOB, 
                           DATEDIFF(YEAR, DOB, GETDATE()) as Age, BloodGroup, Phone, Address
                    FROM Patient WHERE PatientId = @PatientId", connection))
                {
                    patientCommand.Parameters.AddWithValue("@PatientId", patientId);
                    using (var reader = await patientCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            report.Patient = new PatientInfo
                            {
                                PatientId = reader.GetInt32("PatientId"),
                                MMRNo = reader.GetString("MMRNo"),
                                Name = reader.GetString("PatientName"),
                                Gender = reader.GetString("Gender"),
                                DOB = reader.IsDBNull("DOB") ? null : (DateTime?)reader.GetDateTime("DOB"),
                                Age = reader.IsDBNull("Age") ? null : (int?)reader.GetInt32("Age"),
                                BloodGroup = reader.IsDBNull("BloodGroup") ? null : reader.GetString("BloodGroup"),
                                Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                                Address = reader.IsDBNull("Address") ? null : reader.GetString("Address")
                            };
                        }
                    }
                }

                // Get test results
                var sql = @"
                    SELECT lt.TestName, lt.NormalRange, lt.SampleType,
                           lr.ActualValue, lr.LowRange, lr.HighRange, lr.Remarks, lr.DoctorReview, lr.Date,
                           CASE 
                               WHEN lr.ActualValue IS NULL THEN 'Not Available'
                               WHEN lr.ActualValue < lr.LowRange THEN 'Low'
                               WHEN lr.ActualValue > lr.HighRange THEN 'High'
                               ELSE 'Normal'
                           END as ValueStatus,
                           CASE 
                               WHEN lr.ActualValue IS NULL THEN 'black'
                               WHEN lr.ActualValue < lr.LowRange OR lr.ActualValue > lr.HighRange THEN 'red'
                               ELSE 'green'
                           END as StatusColor
                    FROM LabResult lr
                    INNER JOIN LabTest lt ON lr.TestId = lt.TestId
                    WHERE lr.PatientId = @PatientId";
                
                if (fromDate.HasValue)
                    sql += " AND lr.Date >= @FromDate";
                if (toDate.HasValue)
                    sql += " AND lr.Date <= @ToDate";
                
                sql += " ORDER BY lr.Date DESC";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PatientId", patientId);
                    if (fromDate.HasValue)
                        command.Parameters.AddWithValue("@FromDate", fromDate.Value);
                    if (toDate.HasValue)
                        command.Parameters.AddWithValue("@ToDate", toDate.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            report.TestResults.Add(new LabTestResult
                            {
                                TestName = reader.GetString("TestName"),
                                NormalRange = reader.GetString("NormalRange"),
                                SampleType = reader.GetString("SampleType"),
                                ActualValue = reader.IsDBNull("ActualValue") ? null : (decimal?)reader.GetDecimal("ActualValue"),
                                LowRange = reader.IsDBNull("LowRange") ? null : (decimal?)reader.GetDecimal("LowRange"),
                                HighRange = reader.IsDBNull("HighRange") ? null : (decimal?)reader.GetDecimal("HighRange"),
                                Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                                DoctorReview = reader.IsDBNull("DoctorReview") ? null : reader.GetString("DoctorReview"),
                                Date = reader.GetDateTime("Date"),
                                ValueStatus = reader.GetString("ValueStatus"),
                                StatusColor = reader.GetString("StatusColor")
                            });
                        }
                    }
                }
            }

            return report;
        }

        public async Task<List<LabTest>> GetAllLabTestsAsync()
        {
            var tests = new List<LabTest>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM LabTest", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tests.Add(new LabTest
                            {
                                TestId = reader.GetInt32("TestId"),
                                TestName = reader.GetString("TestName"),
                                Price = reader.GetDecimal("Price"),
                                SampleType = reader.GetString("SampleType"),
                                NormalRange = reader.GetString("NormalRange"),
                                Duration = reader.GetString("Duration")
                            });
                        }
                    }
                }
            }
            return tests;
        }

        public async Task<LabResultViewModel> GetLabTestDetailsAsync(int testId, int patientId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
                    SELECT TOP 1
                        ltp.PrescriptionId as ResultId,
                        ltp.TestId,
                        ltp.PatientId,
                        lt.TestName,
                        lt.NormalRange,
                        lt.SampleType,
                        p.Name as PatientName,
                        p.MMRNo,
                        p.Gender,
                        DATEDIFF(YEAR, p.DOB, GETDATE()) as Age,
                        lr.LowRange,
                        lr.HighRange,
                        lr.ActualValue,
                        lr.Remarks,
                        lr.DoctorReview,
                        COALESCE(lr.Date, ltp.PrescribedDate) as Date
                    FROM LabTestPrescription ltp
                    INNER JOIN LabTest lt ON ltp.TestId = lt.TestId
                    INNER JOIN Patient p ON ltp.PatientId = p.PatientId
                    LEFT JOIN LabResult lr ON lr.TestId = ltp.TestId AND lr.PatientId = ltp.PatientId
                    WHERE ltp.TestId = @TestId AND ltp.PatientId = @PatientId
                    ORDER BY ltp.PrescriptionId DESC", connection))
                {
                    command.Parameters.AddWithValue("@TestId", testId);
                    command.Parameters.AddWithValue("@PatientId", patientId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var result = new LabResultViewModel
                            {
                                ResultId = reader.GetInt32("ResultId"),
                                TestId = reader.GetInt32("TestId"),
                                PatientId = reader.GetInt32("PatientId"),
                                LowRange = reader.IsDBNull("LowRange") ? null : (decimal?)reader.GetDecimal("LowRange"),
                                HighRange = reader.IsDBNull("HighRange") ? null : (decimal?)reader.GetDecimal("HighRange"),
                                ActualValue = reader.IsDBNull("ActualValue") ? null : (decimal?)reader.GetDecimal("ActualValue"),
                                Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                                DoctorReview = reader.IsDBNull("DoctorReview") ? null : reader.GetString("DoctorReview"),
                                Date = reader.GetDateTime("Date"),
                                TestName = reader.GetString("TestName"),
                                PatientName = reader.GetString("PatientName"),
                                MMRNo = reader.GetString("MMRNo"),
                                Gender = reader.GetString("Gender"),
                                Age = reader.IsDBNull("Age") ? null : (int?)reader.GetInt32("Age"),
                                NormalRange = reader.IsDBNull("NormalRange") ? null : reader.GetString("NormalRange"),
                                SampleType = reader.IsDBNull("SampleType") ? null : reader.GetString("SampleType")
                            };

                            CalculateValueStatus(result);
                            return result;
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> AddOrUpdateLabResultAsync(LabResultViewModel labResult)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
                    IF EXISTS (SELECT 1 FROM LabResult WHERE TestId = @TestId AND PatientId = @PatientId)
                    BEGIN
                        UPDATE LabResult 
                        SET LowRange = @LowRange, HighRange = @HighRange, ActualValue = @ActualValue,
                            Remarks = @Remarks, DoctorReview = @DoctorReview, Date = GETDATE()
                        WHERE TestId = @TestId AND PatientId = @PatientId
                    END
                    ELSE
                    BEGIN
                        INSERT INTO LabResult (TestId, PatientId, LowRange, HighRange, ActualValue, Remarks, DoctorReview, Date)
                        VALUES (@TestId, @PatientId, @LowRange, @HighRange, @ActualValue, @Remarks, @DoctorReview, GETDATE())
                    END", connection))
                {
                    command.Parameters.AddWithValue("@TestId", labResult.TestId);
                    command.Parameters.AddWithValue("@PatientId", labResult.PatientId);
                    command.Parameters.AddWithValue("@LowRange", labResult.LowRange ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@HighRange", labResult.HighRange ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ActualValue", labResult.ActualValue ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Remarks", labResult.Remarks ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DoctorReview", labResult.DoctorReview ?? (object)DBNull.Value);

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        public async Task<LabTest> GetLabTestByIdAsync(int testId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM LabTest WHERE TestId = @TestId", connection))
                {
                    command.Parameters.AddWithValue("@TestId", testId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new LabTest
                            {
                                TestId = reader.GetInt32("TestId"),
                                TestName = reader.GetString("TestName"),
                                Price = reader.GetDecimal("Price"),
                                SampleType = reader.GetString("SampleType"),
                                NormalRange = reader.GetString("NormalRange"),
                                Duration = reader.GetString("Duration")
                            };
                        }
                    }
                }
            }
            return null;
        }

        private string DetermineTestStatus(decimal? actualValue)
        {
            return actualValue.HasValue ? "Completed" : "Pending";
        }

        private void CalculateValueStatus(LabResultViewModel result)
        {
            if (!result.ActualValue.HasValue || !result.LowRange.HasValue || !result.HighRange.HasValue)
            {
                result.ValueStatus = "Not Available";
                result.StatusColor = "black";
                return;
            }

            var actualValue = result.ActualValue.Value;
            var lowRange = result.LowRange.Value;
            var highRange = result.HighRange.Value;

            if (actualValue < lowRange)
            {
                result.ValueStatus = "Value is Low";
                result.StatusColor = "red";
            }
            else if (actualValue > highRange)
            {
                result.ValueStatus = "Value is High";
                result.StatusColor = "red";
            }
            else
            {
                result.ValueStatus = "Value is Normal";
                result.StatusColor = "green";
            }
        }
    }
}

