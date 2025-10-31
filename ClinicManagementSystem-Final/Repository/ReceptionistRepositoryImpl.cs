using ClinicManagementSystem_Final.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ClinicManagementSystem_Final.Repository
{
    public class ReceptionistRepositoryImpl : IReceptionistRepository
    {
        private readonly string _connectionString;

        public ReceptionistRepositoryImpl(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MVCConnectionString");
        }

        // --------------------- Patient Management ---------------------
        public Patient GetPatientById(int id)
        {
            Patient patient = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetPatientById", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PatientId", id);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        patient = new Patient
                        {
                            PatientId = Convert.ToInt32(reader["PatientId"]),
                            MMRNo = reader["MMRNo"].ToString(),
                            PatientName = reader["Name"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            DOB = reader["DOB"] != DBNull.Value ? Convert.ToDateTime(reader["DOB"]) : DateTime.MinValue,
                            ContactNumber = reader["Phone"].ToString(),
                            Address = reader["Address"].ToString(),
                            BloodGroup = reader["BloodGroup"] != DBNull.Value ? reader["BloodGroup"].ToString() : string.Empty,
                            Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : string.Empty
                        };
                    }
                }
            }

            return patient;
        }

        public void AddPatient(Patient patient)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    
                    // Auto-generate MMRNo
                    string mmrNo = GenerateMMRNo(con);
                    System.Diagnostics.Debug.WriteLine($"Generated MMRNo: {mmrNo}");
                    
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Patient (MMRNo, Name, Gender, DOB, Phone, Address, BloodGroup, Status)
                        VALUES (@MMRNo, @Name, @Gender, @DOB, @Phone, @Address, @BloodGroup, @Status)", con))
                    {
                        cmd.Parameters.AddWithValue("@MMRNo", mmrNo);
                        cmd.Parameters.AddWithValue("@Name", patient.PatientName ?? "");
                        cmd.Parameters.AddWithValue("@Gender", patient.Gender ?? "");
                        cmd.Parameters.AddWithValue("@DOB", patient.DOB);
                        cmd.Parameters.AddWithValue("@Phone", patient.ContactNumber ?? "");
                        cmd.Parameters.AddWithValue("@Address", patient.Address ?? "");
                        cmd.Parameters.AddWithValue("@BloodGroup", string.IsNullOrEmpty(patient.BloodGroup) ? (object)DBNull.Value : patient.BloodGroup);
                        cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(patient.Status) ? "Active" : patient.Status);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"AddPatient: {rowsAffected} rows affected");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddPatient Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw; // Re-throw to let controller handle it
            }
        }

        private string GenerateMMRNo(SqlConnection con)
        {
            try
            {
                // Generate MMRNo in format: MMR + YYYY + 4-digit sequential number
                string year = DateTime.Now.Year.ToString();
                string prefix = $"MMR{year}";
                
                using (var cmd = new SqlCommand($@"
                    SELECT ISNULL(MAX(CAST(SUBSTRING(MMRNo, {prefix.Length + 1}, 4) AS INT)), 0) + 1 
                    FROM Patient 
                    WHERE MMRNo LIKE '{prefix}%'", con))
                {
                    object result = cmd.ExecuteScalar();
                    int nextNumber = result != null ? Convert.ToInt32(result) : 1;
                    string mmrNo = $"{prefix}{nextNumber:D4}";
                    System.Diagnostics.Debug.WriteLine($"Generated MMRNo: {mmrNo}");
                    return mmrNo;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GenerateMMRNo Error: {ex.Message}");
                // Fallback to simple format
                return $"MMR{DateTime.Now.Year}{DateTime.Now.Ticks % 10000:D4}";
            }
        }

        public void UpdatePatient(Patient patient)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Patient 
                    SET Name = @Name, Gender = @Gender, DOB = @DOB, Phone = @Phone, 
                        Address = @Address, BloodGroup = @BloodGroup, Status = @Status
                    WHERE PatientId = @PatientId", con))
                {
                    cmd.Parameters.AddWithValue("@PatientId", patient.PatientId);
                    cmd.Parameters.AddWithValue("@Name", patient.PatientName);
                    cmd.Parameters.AddWithValue("@Gender", patient.Gender);
                    cmd.Parameters.AddWithValue("@DOB", patient.DOB);
                    cmd.Parameters.AddWithValue("@Phone", patient.ContactNumber);
                    cmd.Parameters.AddWithValue("@Address", patient.Address);
                    cmd.Parameters.AddWithValue("@BloodGroup", patient.BloodGroup ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", patient.Status ?? (object)DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeletePatient(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Patient 
                    SET Status = 'Inactive' 
                    WHERE PatientId = @PatientId", con))
                {
                    cmd.Parameters.AddWithValue("@PatientId", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Patient> SearchPatientsByMMRNo(string mmrNo)
        {
            List<Patient> patients = new List<Patient>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_SearchPatientByMMRNo", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MMRNo", mmrNo);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        patients.Add(new Patient
                        {
                            PatientId = Convert.ToInt32(reader["PatientId"]),
                            MMRNo = reader["MMRNo"].ToString(),
                            PatientName = reader["Name"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            DOB = reader["DOB"] != DBNull.Value ? Convert.ToDateTime(reader["DOB"]) : DateTime.MinValue,
                            ContactNumber = reader["Phone"].ToString(),
                            Address = reader["Address"].ToString(),
                            BloodGroup = reader["BloodGroup"] != DBNull.Value ? reader["BloodGroup"].ToString() : string.Empty,
                            Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : string.Empty
                        });
                    }
                }
            }

            return patients;
        }

        public List<Patient> SearchPatientsByPhone(string phone)
        {
            List<Patient> patients = new List<Patient>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT PatientId, MMRNo, Name, Gender, DOB, Phone, Address, BloodGroup, Status
                               FROM Patient
                               WHERE Phone LIKE '%' + @Phone + '%' AND Status='Active'";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Phone", phone);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        patients.Add(new Patient
                        {
                            PatientId = Convert.ToInt32(reader["PatientId"]),
                            MMRNo = reader["MMRNo"].ToString(),
                            PatientName = reader["Name"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            DOB = reader["DOB"] != DBNull.Value ? Convert.ToDateTime(reader["DOB"]) : DateTime.MinValue,
                            ContactNumber = reader["Phone"].ToString(),
                            Address = reader["Address"].ToString(),
                            BloodGroup = reader["BloodGroup"] != DBNull.Value ? reader["BloodGroup"].ToString() : string.Empty,
                            Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : string.Empty
                        });
                    }
                }
            }
            return patients;
        }

        public List<Patient> GetAllPatients()
        {
            List<Patient> patients = new List<Patient>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT PatientId, MMRNo, Name, Gender, DOB, Phone, Address, BloodGroup, Status
                               FROM Patient
                               WHERE Status = 'Active'";

                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        patients.Add(new Patient
                        {
                            PatientId = Convert.ToInt32(reader["PatientId"]),
                            MMRNo = reader["MMRNo"].ToString(),
                            PatientName = reader["Name"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            DOB = reader["DOB"] != DBNull.Value ? Convert.ToDateTime(reader["DOB"]) : DateTime.MinValue,
                            ContactNumber = reader["Phone"].ToString(),
                            Address = reader["Address"].ToString(),
                            BloodGroup = reader["BloodGroup"] != DBNull.Value ? reader["BloodGroup"].ToString() : string.Empty,
                            Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : string.Empty
                        });
                    }
                }
            }
            return patients;
        }

        // --------------------- Doctors & Appointments ---------------------
        public List<Doctor> GetAllDoctors()
        {
            List<Doctor> doctors = new List<Doctor>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT d.DoctorId, s.Name AS DoctorName, d.Specialization, s.Phone AS ContactNumber, s.Email, s.Status
                               FROM Doctor d
                               INNER JOIN Staff s ON d.StaffId = s.StaffId
                               WHERE s.Status = 'Active'";

                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        doctors.Add(new Doctor
                        {
                            DoctorId = Convert.ToInt32(reader["DoctorId"]),
                            DoctorName = reader["DoctorName"].ToString(),
                            Specialization = new Specialization { Name = reader["Specialization"] != DBNull.Value ? reader["Specialization"].ToString() : "General" },
                            ContactNumber = reader["ContactNumber"]?.ToString(),
                            Email = reader["Email"]?.ToString(),
                            IsActive = reader["Status"].ToString() == "Active"
                        });
                    }
                }
            }

            return doctors;
        }

        public List<AppointmentViewModel> GetTodayAppointments()
        {
            List<AppointmentViewModel> appointments = new List<AppointmentViewModel>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(@"
                SELECT 
                    a.AppointmentId,
                    a.TokenNumber,
                    p.Name AS PatientName,
                    s.Name AS DoctorName,
                    a.AppointmentDate,
                    a.Status,
                    p.MMRNo,
                    p.Gender,
                    DATEDIFF(YEAR, p.DOB, GETDATE()) AS Age,
                    d.Specialization
                FROM Appointment a
                INNER JOIN Patient p ON a.PatientId = p.PatientId
                INNER JOIN Doctor d ON a.DoctorId = d.DoctorId
                INNER JOIN Staff s ON d.StaffId = s.StaffId
                WHERE CAST(a.AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY a.AppointmentDate ASC", con);


                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    appointments.Add(new AppointmentViewModel
                    {
                        AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                        TokenNumber = reader["TokenNumber"] != DBNull.Value ? Convert.ToInt32(reader["TokenNumber"]) : 0,
                        PatientName = reader["PatientName"].ToString(),
                        DoctorName = reader["DoctorName"].ToString(),
                        AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                        Status = reader["Status"].ToString(),
                        MMRNo = reader["MMRNo"].ToString(),
                        Gender = reader["Gender"] != DBNull.Value ? reader["Gender"].ToString() : "",
                        Age = reader["Age"] != DBNull.Value ? Convert.ToInt32(reader["Age"]) : 0,
                        Specialization = reader["Specialization"] != DBNull.Value ? reader["Specialization"].ToString() : "General"
                    });
                }
            }
            return appointments;
        }


        public (int appointmentId, int tokenNumber) BookAppointment(int patientId, int doctorId, DateTime appointmentDate)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"BookAppointment called: PatientId={patientId}, DoctorId={doctorId}, AppointmentDate={appointmentDate}");
                
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    System.Diagnostics.Debug.WriteLine($"Connected to database: {con.Database}");
                    
                    // Check Appointment table structure
                    using (var checkCmd = new SqlCommand(@"
                        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Appointment' 
                        ORDER BY ORDINAL_POSITION", con))
                    {
                        System.Diagnostics.Debug.WriteLine("Appointment table structure:");
                        using (var reader = checkCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine($"  {reader["COLUMN_NAME"]} - {reader["DATA_TYPE"]} - Nullable: {reader["IS_NULLABLE"]}");
                            }
                        }
                    }
                    
                    // Verify patient and doctor exist - simplified approach
                    using (var patientCmd = new SqlCommand("SELECT COUNT(*) FROM Patient WHERE PatientId = @PatientId AND Status = 'Active'", con))
                    {
                        patientCmd.Parameters.AddWithValue("@PatientId", patientId);
                        var patientCount = Convert.ToInt32(patientCmd.ExecuteScalar());
                        
                        using (var doctorCmd = new SqlCommand(@"
                            SELECT COUNT(*) FROM Doctor d 
                            INNER JOIN Staff s ON d.StaffId = s.StaffId 
                            WHERE d.DoctorId = @DoctorId AND s.Status = 'Active'", con))
                        {
                            doctorCmd.Parameters.AddWithValue("@DoctorId", doctorId);
                            var doctorCount = Convert.ToInt32(doctorCmd.ExecuteScalar());
                            
                            System.Diagnostics.Debug.WriteLine($"Patient exists: {patientCount > 0}, Doctor exists: {doctorCount > 0}");
                            
                            if (patientCount == 0)
                            {
                                System.Diagnostics.Debug.WriteLine("ERROR: Patient not found or inactive");
                                return (0, 0);
                            }
                            if (doctorCount == 0)
                            {
                                System.Diagnostics.Debug.WriteLine("ERROR: Doctor not found or inactive");
                                return (0, 0);
                            }
                        }
                    }
                    
                    // Get next token number for today
                    int tokenNumber = 1;
                    using (var cmd = new SqlCommand(@"
                        SELECT ISNULL(MAX(TokenNumber), 0) + 1 
                        FROM Appointment 
                        WHERE CAST(AppointmentDate AS DATE) = CAST(@AppointmentDate AS DATE)", con))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            tokenNumber = Convert.ToInt32(result);
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Generated token number: {tokenNumber}");
                    
                    // Insert appointment - try different approaches
                    try
                    {
                        // First try: Standard columns
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO Appointment (PatientId, DoctorId, AppointmentDate, Status, TokenNumber)
                            VALUES (@PatientId, @DoctorId, @AppointmentDate, 'Scheduled', @TokenNumber);
                            SELECT SCOPE_IDENTITY();", con))
                        {
                            cmd.Parameters.AddWithValue("@PatientId", patientId);
                            cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                            cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate);
                            cmd.Parameters.AddWithValue("@TokenNumber", tokenNumber);
                            
                            object appointmentIdResult = cmd.ExecuteScalar();
                            int appointmentId = appointmentIdResult != null ? Convert.ToInt32(appointmentIdResult) : 0;
                            
                            System.Diagnostics.Debug.WriteLine($"Insert result (standard): AppointmentId={appointmentId}, Token={tokenNumber}");
                            return (appointmentId, tokenNumber);
                        }
                    }
                    catch (Exception insertEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Standard insert failed: {insertEx.Message}");
                        
                        // Try alternative: Maybe different column names
                        try
                        {
                            using (var cmd = new SqlCommand(@"
                                INSERT INTO Appointment (PatientId, DoctorId, AppointmentDateTime, Status, TokenNumber)
                                VALUES (@PatientId, @DoctorId, @AppointmentDate, 'Scheduled', @TokenNumber);
                                SELECT SCOPE_IDENTITY();", con))
                            {
                                cmd.Parameters.AddWithValue("@PatientId", patientId);
                                cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                                cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate);
                                cmd.Parameters.AddWithValue("@TokenNumber", tokenNumber);
                                
                                object appointmentIdResult = cmd.ExecuteScalar();
                                int appointmentId = appointmentIdResult != null ? Convert.ToInt32(appointmentIdResult) : 0;
                                
                                System.Diagnostics.Debug.WriteLine($"Insert result (alternative): AppointmentId={appointmentId}, Token={tokenNumber}");
                                return (appointmentId, tokenNumber);
                            }
                        }
                        catch (Exception altEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Alternative insert failed: {altEx.Message}");
                            throw; // Re-throw the original exception
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BookAppointment Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return (0, 0);
            }
        }

        public AppointmentBillViewModel GetAppointmentBill(int appointmentId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetAppointmentBill called with AppointmentId: {appointmentId}");
                
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string sql = @"
                        SELECT 
                            a.AppointmentId,
                            a.TokenNumber,
                            a.AppointmentDate,
                            p.Name AS PatientName,
                            p.MMRNo,
                            p.Gender,
                            DATEDIFF(YEAR, p.DOB, GETDATE()) AS Age,
                            s.Name AS DoctorName,
                            d.Specialization,
                            500 AS ConsultationFee
                        FROM Appointment a
                        INNER JOIN Patient p ON a.PatientId = p.PatientId
                        INNER JOIN Doctor d ON a.DoctorId = d.DoctorId
                        INNER JOIN Staff s ON d.StaffId = s.StaffId
                        WHERE a.AppointmentId = @AppointmentId";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                        con.Open();
                        
                        System.Diagnostics.Debug.WriteLine($"Executing SQL query for appointment {appointmentId}");
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var bill = new AppointmentBillViewModel
                                {
                                    AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                                    TokenNumber = Convert.ToInt32(reader["TokenNumber"]),
                                    AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                                    PatientName = reader["PatientName"].ToString(),
                                    MMRNo = reader["MMRNo"].ToString(),
                                    Gender = reader["Gender"] != DBNull.Value ? reader["Gender"].ToString() : "",
                                    Age = reader["Age"] != DBNull.Value ? Convert.ToInt32(reader["Age"]) : 0,
                                    DoctorName = reader["DoctorName"].ToString(),
                                    Specialization = reader["Specialization"] != DBNull.Value ? reader["Specialization"].ToString() : "General",
                                    ConsultationFee = 500, // Fixed fee since Fee column doesn't exist
                                    TimeSlot = Convert.ToDateTime(reader["AppointmentDate"]).ToString("hh:mm tt"),
                                    BillDate = DateTime.Now
                                };
                                
                                System.Diagnostics.Debug.WriteLine($"Bill created successfully: Patient={bill.PatientName}, Doctor={bill.DoctorName}, Token={bill.TokenNumber}");
                                return bill;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"No data found for appointment {appointmentId}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAppointmentBill: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            
            return null;
        }
    }
}
