using ClinicManagementSystem_Final.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ClinicManagementSystem_Final.Repository
{
    public class DoctorRepositoryImpl : IDoctorRepository
    {
        private readonly string _connectionString;

        public DoctorRepositoryImpl(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MVCConnectionString");
        }

        public List<DoctorDashboardViewModel> GetAppointments(int doctorId, DateTime forDate)
        {
            List<DoctorDashboardViewModel> appointments = new List<DoctorDashboardViewModel>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Doctor_GetTodaysAppointments", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                cmd.Parameters.AddWithValue("@ForDate", forDate);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DoctorDashboardViewModel appointment = new DoctorDashboardViewModel()
                        {
                            AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                            TokenNumber = Convert.ToInt32(reader["TokenNumber"]),
                            AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                            Status = reader["Status"]?.ToString(),
                            MMRNo = reader["MMRNo"]?.ToString(),
                            PatientName = reader["PatientName"]?.ToString(),
                            Gender = reader["Gender"]?.ToString(),
                            Age = reader["Age"] != DBNull.Value ? Convert.ToInt32(reader["Age"]) : (int?)null
                        };
                        appointments.Add(appointment);
                    }
                    con.Close();
                }
                return appointments;
            }
        }

        public int? GetDoctorIdByStaffId(int staffId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = "SELECT DoctorId FROM Doctor WHERE StaffId = @StaffId";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@StaffId", staffId);

                con.Open();
                var result = cmd.ExecuteScalar();
                con.Close();

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
                return null;
            }
        }

        public PatientDetailViewModel GetPatientByMMR(string mmrNo)
        {
            var model = new PatientDetailViewModel();

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_Doctor_GetPatientByMMR", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MMRNo", mmrNo ?? string.Empty);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // ---- Result set 1: patient basic info ----
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            model.PatientId = reader["PatientId"] != DBNull.Value ? Convert.ToInt32(reader["PatientId"]) : 0;
                            model.MMRNo = reader["MMRNo"]?.ToString();
                            model.Name = reader["Name"]?.ToString();
                            model.Address = reader["Address"]?.ToString();
                            model.Gender = reader["Gender"]?.ToString();
                            model.DOB = reader["DOB"] != DBNull.Value ? Convert.ToDateTime(reader["DOB"]) : (DateTime?)null;
                            model.Age = reader["Age"] != DBNull.Value ? Convert.ToInt32(reader["Age"]) : (int?)null;
                            model.BloodGroup = reader["BloodGroup"]?.ToString();
                            model.Phone = reader["Phone"]?.ToString();
                            model.Status = reader["Status"]?.ToString();
                        }
                    }

                    // ---- Result set 2: recent consultations ----
                    if (reader.NextResult())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var history = new DiagnosisHistoryViewModel()
                                {
                                    DiagnosisId = reader["DiagnosisId"] != DBNull.Value ? Convert.ToInt32(reader["DiagnosisId"]) : 0,
                                    AppointmentId = reader["AppointmentId"] != DBNull.Value ? Convert.ToInt32(reader["AppointmentId"]) : 0,
                                    Date = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : (DateTime?)null,
                                    Symptoms = reader["Symptoms"]?.ToString(),
                                    Diagnosis = reader["Diagnosis"]?.ToString(),
                                    PrescribedMedicine = reader["PrescribedMedicine"]?.ToString(),
                                    PrescribedLab = reader["PrescribedLab"]?.ToString(),
                                    DoctorNotes = reader["DoctorNotes"]?.ToString(),
                                    AppointmentDate = reader["AppointmentDate"] != DBNull.Value ? Convert.ToDateTime(reader["AppointmentDate"]) : (DateTime?)null,
                                    WhoDoctorId = reader["WhoDoctorId"] != DBNull.Value ? Convert.ToInt32(reader["WhoDoctorId"]) : (int?)null,
                                    WhoDoctorName = reader["WhoDoctorName"]?.ToString()
                                };
                                model.Consultations.Add(history);
                            }
                        }
                    }

                    // ---- Result set 3: lab results ----
                    if (reader.NextResult())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var lr = new LabResultViewModel()
                                {
                                    ResultId = reader["ResultId"] != DBNull.Value ? Convert.ToInt32(reader["ResultId"]) : 0,
                                    TestName = reader["TestName"]?.ToString(),
                                    LowRange = reader["LowRange"] != DBNull.Value ? Convert.ToDecimal(reader["LowRange"]) : (decimal?)null,
                                    HighRange = reader["HighRange"] != DBNull.Value ? Convert.ToDecimal(reader["HighRange"]) : (decimal?)null,
                                    ActualValue = reader["ActualValue"] != DBNull.Value ? Convert.ToDecimal(reader["ActualValue"]) : (decimal?)null,
                                    Remarks = reader["Remarks"]?.ToString(),
                                    DoctorReview = reader["DoctorReview"]?.ToString(),
                                    Date = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : DateTime.Now
                                };
                                model.LabResults.Add(lr);
                            }
                        }
                    }
                }
                con.Close();
            }

            return model;
        }

        // Updated GetConsultationByAppointment to consume the modified stored-proc (aggregated medicine/lab columns)
        public ConsultationViewModel GetConsultationByAppointment(int appointmentId)
        {
            var result = new ConsultationViewModel();

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_Doctor_GetConsultationByAppointment", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // First result set: appointment + patient info
                    if (reader.Read())
                    {
                        result.AppointmentId = reader["AppointmentId"] != DBNull.Value ? Convert.ToInt32(reader["AppointmentId"]) : 0;
                        result.PatientId = reader["PatientId"] != DBNull.Value ? Convert.ToInt32(reader["PatientId"]) : 0;
                        result.DoctorId = reader["DoctorId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorId"]) : (int?)null;
                        result.TokenNumber = reader["TokenNumber"] != DBNull.Value ? Convert.ToInt32(reader["TokenNumber"]) : 0;
                        result.AppointmentDate = reader["AppointmentDate"] != DBNull.Value ? Convert.ToDateTime(reader["AppointmentDate"]) : DateTime.MinValue;
                        result.Status = reader["Status"]?.ToString();
                        result.MMRNo = reader["MMRNo"]?.ToString();
                        result.PatientName = reader["PatientName"]?.ToString();
                        result.Gender = reader["Gender"]?.ToString();
                        result.Age = reader["Age"] != DBNull.Value ? Convert.ToInt32(reader["Age"]) : (int?)null;
                    }

                    // Move to second result set: existing diagnosis (if any) with aggregated medicines & labs
                    if (reader.NextResult())
                    {
                        if (reader.Read())
                        {
                            result.ExistingDiagnosis = new DiagnosisHistoryViewModel()
                            {
                                DiagnosisId = reader["DiagnosisId"] != DBNull.Value ? Convert.ToInt32(reader["DiagnosisId"]) : 0,
                                Symptoms = reader["Symptoms"]?.ToString(),
                                Diagnosis = reader["Diagnosis"]?.ToString(),
                                // PrescribedMedicine and PrescribedLab now come from aggregated MedPrescription / LabTestPrescription
                                PrescribedMedicine = reader["PrescribedMedicine"] != DBNull.Value ? reader["PrescribedMedicine"].ToString() : string.Empty,
                                PrescribedLab = reader["PrescribedLab"] != DBNull.Value ? reader["PrescribedLab"].ToString() : string.Empty,
                                DoctorNotes = reader["DoctorNotes"]?.ToString(),
                                Date = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : (DateTime?)null
                            };
                        }
                    }
                }
                con.Close();
            }

            return result;
        }

        public int SaveConsultation(int appointmentId, int patientId, int doctorId,
            string symptoms, string diagnosis, string prescribedMedicine, string prescribedLab, string doctorNotes)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_Doctor_SaveConsultation", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // pass the parameters (stored-proc will store DiagnosisDetails without prescription columns)
                cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                cmd.Parameters.AddWithValue("@PatientId", patientId);
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                cmd.Parameters.AddWithValue("@Symptoms", (object)symptoms ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Diagnosis", (object)diagnosis ?? DBNull.Value);
                // keep compatibility parameters (SP ignores them)
                cmd.Parameters.AddWithValue("@PrescribedMedicine", (object)prescribedMedicine ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PrescribedLab", (object)prescribedLab ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DoctorNotes", (object)doctorNotes ?? DBNull.Value);

                var outParam = new SqlParameter("@OutStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(outParam);

                con.Open();
                cmd.ExecuteNonQuery();

                int status = outParam.Value != DBNull.Value ? Convert.ToInt32(outParam.Value) : 2;

                // If stored-proc succeeded, parse prescribedMedicine and insert into MedPrescription table
                if (status == 0)
                {
                    using (var tx = con.BeginTransaction())
                    {
                        try
                        {
                            // Insert MedPrescription rows (if any)
                            if (!string.IsNullOrWhiteSpace(prescribedMedicine))
                            {
                                string[] medEntries = prescribedMedicine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var me in medEntries)
                                {
                                    // expected format: MedicineId:Qty:Frequency:DurationDays  (from client)
                                    var parts = me.Split(':');
                                    if (parts.Length < 2) continue;
                                    if (!int.TryParse(parts[0], out int medicineId)) continue;
                                    if (!int.TryParse(parts[1], out int qtyIssued)) continue;
                                    var frequency = parts.Length > 2 ? parts[2] : null;
                                    int durationDays = 0;
                                    if (parts.Length > 3) int.TryParse(parts[3], out durationDays);

                                    using (SqlCommand ins = new SqlCommand(@"
                                        INSERT INTO MedPrescription
                                            (AppointmentId, PatientId, DoctorId, MedicineId, Quantity, Frequency, DurationDays, PrescribedDate, Status)
                                        VALUES
                                            (@AppointmentId, @PatientId, @DoctorId, @MedicineId, @Quantity, @Frequency, @DurationDays, GETDATE(), @Status)
                                    ", con, tx))
                                    {
                                        ins.Parameters.AddWithValue("@AppointmentId", appointmentId);
                                        ins.Parameters.AddWithValue("@PatientId", patientId);
                                        ins.Parameters.AddWithValue("@DoctorId", doctorId);
                                        ins.Parameters.AddWithValue("@MedicineId", medicineId);
                                        ins.Parameters.AddWithValue("@Quantity", qtyIssued);
                                        ins.Parameters.AddWithValue("@Frequency", (object)frequency ?? DBNull.Value);
                                        ins.Parameters.AddWithValue("@DurationDays", durationDays);
                                        ins.Parameters.AddWithValue("@Status", "Pending");

                                        ins.ExecuteNonQuery();
                                    }
                                }
                            }

                            // Insert LabTestPrescription rows (if any)
                            if (!string.IsNullOrWhiteSpace(prescribedLab))
                            {
                                string[] labEntries = prescribedLab.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var le in labEntries)
                                {
                                    var token = le.Trim();
                                    if (string.IsNullOrEmpty(token)) continue;

                                    int? testId = null;
                                    string testName = token;
                                    int quantity = 1;

                                    // Accept formats: "TestId" or "TestName" or "TestName:Qty"
                                    var parts = token.Split(':');
                                    if (parts.Length >= 1)
                                    {
                                        // try parse test id
                                        if (int.TryParse(parts[0].Trim(), out int parsedTestId))
                                        {
                                            testId = parsedTestId;
                                            // lookup name if needed
                                            using (var lookup = new SqlCommand("SELECT TestName FROM LabTest WHERE TestId = @Id", con, tx))
                                            {
                                                lookup.Parameters.AddWithValue("@Id", testId.Value);
                                                var res = lookup.ExecuteScalar();
                                                if (res != null && res != DBNull.Value)
                                                    testName = res.ToString();
                                            }
                                        }
                                        else
                                        {
                                            testName = parts[0].Trim();
                                            // try lookup id by name (best-effort)
                                            using (var lookup = new SqlCommand("SELECT TestId FROM LabTest WHERE TestName = @Name", con, tx))
                                            {
                                                lookup.Parameters.AddWithValue("@Name", testName);
                                                var res = lookup.ExecuteScalar();
                                                if (res != null && res != DBNull.Value)
                                                    testId = Convert.ToInt32(res);
                                            }
                                        }

                                        if (parts.Length > 1)
                                        {
                                            int.TryParse(parts[1], out quantity);
                                            if (quantity <= 0) quantity = 1;
                                        }
                                    }

                                    using (SqlCommand lins = new SqlCommand(@"
                                        INSERT INTO LabTestPrescription
                                            (AppointmentId, PatientId, DoctorId, TestId, TestName, Quantity, PrescribedDate, Status)
                                        VALUES
                                            (@AppointmentId, @PatientId, @DoctorId, @TestId, @TestName, @Quantity, GETDATE(), @Status)
                                    ", con, tx))
                                    {
                                        lins.Parameters.AddWithValue("@AppointmentId", appointmentId);
                                        lins.Parameters.AddWithValue("@PatientId", patientId);
                                        lins.Parameters.AddWithValue("@DoctorId", doctorId);
                                        lins.Parameters.AddWithValue("@TestId", testId.HasValue ? (object)testId.Value : DBNull.Value);
                                        lins.Parameters.AddWithValue("@TestName", (object)testName ?? DBNull.Value);
                                        lins.Parameters.AddWithValue("@Quantity", quantity);
                                        lins.Parameters.AddWithValue("@Status", "Pending");

                                        lins.ExecuteNonQuery();
                                    }
                                }
                            }

                            tx.Commit();
                        }
                        catch
                        {
                            try { tx.Rollback(); } catch { }
                            // consider logging the error
                        }
                    }
                }

                con.Close();
                return status;
            }
        }

        public void UpdateAppointmentStatus(int appointmentId, string newStatus)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_Doctor_UpdateAppointmentStatus", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                cmd.Parameters.AddWithValue("@NewStatus", newStatus ?? (object)DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        public List<LabResultViewModel> GetLabResultsForPatient(int patientId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var list = new List<LabResultViewModel>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_Doctor_GetLabResultsForPatient", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PatientId", patientId);
                cmd.Parameters.AddWithValue("@FromDate", (object)fromDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", (object)toDate ?? DBNull.Value);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var lr = new LabResultViewModel()
                        {
                            ResultId = reader["ResultId"] != DBNull.Value ? Convert.ToInt32(reader["ResultId"]) : 0,
                            TestName = reader["TestName"]?.ToString(),
                            LowRange = reader["LowRange"] != DBNull.Value ? Convert.ToDecimal(reader["LowRange"]) : (decimal?)null,
                            HighRange = reader["HighRange"] != DBNull.Value ? Convert.ToDecimal(reader["HighRange"]) : (decimal?)null,
                            ActualValue = reader["ActualValue"] != DBNull.Value ? Convert.ToDecimal(reader["ActualValue"]) : (decimal?)null,
                            Remarks = reader["Remarks"]?.ToString(),
                            DoctorReview = reader["DoctorReview"]?.ToString(),
                            Date = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : DateTime.Now
                        };
                        list.Add(lr);
                    }
                }
                con.Close();
            }

            return list;
        }

        public List<MedicineViewModel> GetAvailableMedicines()
        {
            var list = new List<MedicineViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT MedicineId, MedicineName, Quantity, Price FROM Medicine WHERE Quantity > 0 ORDER BY MedicineName", con))
            {
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new MedicineViewModel
                        {
                            MedicineId = reader["MedicineId"] != DBNull.Value ? Convert.ToInt32(reader["MedicineId"]) : 0,
                            MedicineName = reader["MedicineName"]?.ToString(),
                            Quantity = reader["Quantity"] != DBNull.Value ? Convert.ToInt32(reader["Quantity"]) : 0,
                            Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0
                        });
                    }
                }
                con.Close();
            }
            return list;
        }

        public List<LabTestViewModel> GetAllLabTests()
        {
            var list = new List<LabTestViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT TestId, TestName, Price FROM LabTest ORDER BY TestName", con))
            {
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new LabTestViewModel
                        {
                            TestId = reader["TestId"] != DBNull.Value ? Convert.ToInt32(reader["TestId"]) : 0,
                            TestName = reader["TestName"]?.ToString(),
                            Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : (decimal?)null
                        });
                    }
                }
                con.Close();
            }
            return list;
        }

    
        
        public Doctor GetDoctorByStaffId(int staffId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT d.DoctorId, d.StaffId, d.SpecializationId, s.Name AS SpecializationName, d.Fee
                       FROM Doctor d
                       LEFT JOIN Specialization s ON d.SpecializationId = s.SpecializationId
                       WHERE d.StaffId = @StaffId";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@StaffId", staffId);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Doctor
                        {
                            DoctorId = Convert.ToInt32(reader["DoctorId"]),
                            StaffId = staffId,
                            SpecializationId = reader["SpecializationId"] != DBNull.Value ? Convert.ToInt32(reader["SpecializationId"]) : 0,
                            Specialization = new Specialization
                            {
                                Name = reader["SpecializationName"]?.ToString()
                            },
                            Fee = reader["Fee"] != DBNull.Value ? Convert.ToDecimal(reader["Fee"]) : (decimal?)null
                        };
                    }
                }
                con.Close();
            }
            return null;
        }
    }
}

