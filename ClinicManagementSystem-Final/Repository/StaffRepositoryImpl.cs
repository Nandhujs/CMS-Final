    using ClinicManagementSystem_Final.Models;
    using ClinicManagementSystem_Final.Repository;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    namespace ClinicManagementSystem_Final.Repository
    {
        public class StaffRepositoryImpl : IStaffRepository
        {
            private readonly string connectionString;

            public StaffRepositoryImpl(IConfiguration configuration)
            {
                connectionString = configuration.GetConnectionString("MVCConnectionString");
            }

            #region List
            public IEnumerable<Staff> GetAllStaff()
            {
                List<Staff> staffList = new List<Staff>();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_GetAllStaff", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Staff staff = new Staff
                            {
                                StaffId = Convert.ToInt32(reader["StaffId"]),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                DOB = Convert.ToDateTime(reader["DOB"]),
                                DOJ = Convert.ToDateTime(reader["DOJ"]),
                                Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null,
                                Gender = reader["Gender"] != DBNull.Value ? reader["Gender"].ToString() : null,
                                Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : null,
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                Role = new Role
                                {
                                    RoleId = Convert.ToInt32(reader["RoleId"]),
                                    RoleName = reader["RoleName"].ToString()
                                }
                            };

                            staffList.Add(staff);
                        }
                    }
                    con.Close();
                }

                return staffList;
            }
            #endregion

            #region Add
            // Add staff and return new StaffId
            public int AddStaff(Staff staff)
            {
                int newStaffId = 0;

                // Validate date ranges for SQL Server compatibility
                var minSqlDate = new DateTime(1753, 1, 1);
                var maxSqlDate = new DateTime(9999, 12, 31);
                
                if (staff.DOB < minSqlDate || staff.DOB > maxSqlDate)
                {
                    throw new ArgumentException($"Date of Birth must be between {minSqlDate:yyyy-MM-dd} and {maxSqlDate:yyyy-MM-dd}");
                }
                
                if (staff.DOJ < minSqlDate || staff.DOJ > maxSqlDate)
                {
                    throw new ArgumentException($"Date of Joining must be between {minSqlDate:yyyy-MM-dd} and {maxSqlDate:yyyy-MM-dd}");
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_AddStaff", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Name", staff.Name);
                    cmd.Parameters.AddWithValue("@Email", staff.Email);
                    cmd.Parameters.AddWithValue("@DOB", staff.DOB);
                    cmd.Parameters.AddWithValue("@DOJ", staff.DOJ);
                    cmd.Parameters.AddWithValue("@Address", (object)staff.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Gender", (object)staff.Gender ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", (object)staff.Phone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoleId", staff.RoleId);
                    cmd.Parameters.AddWithValue("@Username", staff.Username);
                    cmd.Parameters.AddWithValue("@Password", staff.Password);
                    cmd.Parameters.AddWithValue("@Status", (object)staff.Status ?? DBNull.Value);

                    con.Open();

                    // ExecuteScalar returns the first column of the first row - your newly inserted StaffId
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        newStaffId = Convert.ToInt32(result);
                    }

                    con.Close();
                }

                return newStaffId;
            }

            public IEnumerable<Role> GetAllRoles()
            {
                var roles = new List<Role>();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT RoleId, RoleName FROM Roles";
                    SqlCommand cmd = new SqlCommand(query, con);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var role = new Role
                            {
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                RoleName = reader["RoleName"].ToString()
                            };
                            roles.Add(role);
                        }
                    }
                    con.Close();
                }

                return roles;
            }

            public IEnumerable<Specialization> GetAllSpecializations()
            {
                var specializations = new List<Specialization>();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT SpecializationId, Name FROM Specialization";
                    SqlCommand cmd = new SqlCommand(query, con);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            specializations.Add(new Specialization
                            {
                                SpecializationId = Convert.ToInt32(reader["SpecializationId"]),
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
                return specializations;
            }

            public void AddDoctorDetails(int staffId, int? specializationId, decimal? fee)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"
                    INSERT INTO Doctor (StaffId, SpecializationId, DeptId)
                    VALUES (@StaffId, @SpecializationId, NULL);
                    ";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@StaffId", staffId);
                    cmd.Parameters.AddWithValue("@SpecializationId", specializationId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
           
            #endregion

            #region update
            public Staff GetStaffById(int staffId)
            {
                Staff staff = null;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT s.*, r.RoleName FROM Staff s
                                     INNER JOIN Roles r ON s.RoleId = r.RoleId
                                     WHERE s.StaffId = @StaffId";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@StaffId", staffId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            staff = new Staff
                            {
                                StaffId = Convert.ToInt32(reader["StaffId"]),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                DOB = Convert.ToDateTime(reader["DOB"]),
                                DOJ = Convert.ToDateTime(reader["DOJ"]),
                                Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null,
                                Gender = reader["Gender"] != DBNull.Value ? reader["Gender"].ToString() : null,
                                Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : null,
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                Role = new Role
                                {
                                    RoleId = Convert.ToInt32(reader["RoleId"]),
                                    RoleName = reader["RoleName"].ToString()
                                }
                            };
                        }
                    }
                    con.Close();
                }
                return staff;
            }

            public void UpdateStaff(Staff staff)
            {
                // Validate date ranges for SQL Server compatibility
                var minSqlDate = new DateTime(1753, 1, 1);
                var maxSqlDate = new DateTime(9999, 12, 31);
                
                if (staff.DOB < minSqlDate || staff.DOB > maxSqlDate)
                {
                    throw new ArgumentException($"Date of Birth must be between {minSqlDate:yyyy-MM-dd} and {maxSqlDate:yyyy-MM-dd}");
                }
                
                if (staff.DOJ < minSqlDate || staff.DOJ > maxSqlDate)
                {
                    throw new ArgumentException($"Date of Joining must be between {minSqlDate:yyyy-MM-dd} and {maxSqlDate:yyyy-MM-dd}");
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE Staff SET
                            Name = @Name,
                            Email = @Email,
                            DOB = @DOB,
                            DOJ = @DOJ,
                            Address = @Address,
                            Gender = @Gender,
                            Phone = @Phone,
                            RoleId = @RoleId,
                            Username = @Username,
                            Password = @Password,
                            Status = @Status
                        WHERE StaffId = @StaffId";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Name", staff.Name);
                    cmd.Parameters.AddWithValue("@Email", staff.Email);
                    cmd.Parameters.AddWithValue("@DOB", staff.DOB);
                    cmd.Parameters.AddWithValue("@DOJ", staff.DOJ);
                    cmd.Parameters.AddWithValue("@Address", (object)staff.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Gender", (object)staff.Gender ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", (object)staff.Phone ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoleId", staff.RoleId);
                    cmd.Parameters.AddWithValue("@Username", staff.Username);
                    cmd.Parameters.AddWithValue("@Password", staff.Password);
                    cmd.Parameters.AddWithValue("@Status", (object)staff.Status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@StaffId", staff.StaffId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }

        public void UpdateDoctorDetails(int staffId, int? specializationId, decimal? fee)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
            UPDATE Doctor 
            SET SpecializationId = @SpecializationId
            WHERE StaffId = @StaffId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@StaffId", staffId);
                cmd.Parameters.AddWithValue("@SpecializationId", specializationId);

                con.Open();
                int rowsUpdated = cmd.ExecuteNonQuery();

                if (rowsUpdated == 0)
                {
                    // Insert if doctor details do not exist (staff role changed to doctor)
                    string insertQuery = @"
                INSERT INTO Doctor (StaffId, SpecializationId, DeptId)
                VALUES (@StaffId, @SpecializationId, NULL)";
                    cmd.CommandText = insertQuery;
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }
        #endregion

        #region
        // in active staff
        public void DisableStaff(int staffId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateStaffStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StaffId", staffId);
                cmd.Parameters.AddWithValue("@Status", "Disabled");
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }
        #endregion
        #region
        public IEnumerable<Staff> GetStaffByMobileNumber(string mobileNumber)
        {
            var staffList = new List<Staff>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT s.*, r.RoleName FROM Staff s
                         INNER JOIN Roles r ON s.RoleId = r.RoleId
                         WHERE s.Phone LIKE @MobileNumber AND s.Status != 'Disabled'";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@MobileNumber", $"%{mobileNumber}%");

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Staff staff = new Staff
                        {
                            StaffId = Convert.ToInt32(reader["StaffId"]),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            DOB = Convert.ToDateTime(reader["DOB"]),
                            DOJ = Convert.ToDateTime(reader["DOJ"]),
                            Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null,
                            Gender = reader["Gender"] != DBNull.Value ? reader["Gender"].ToString() : null,
                            Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : null,
                            RoleId = Convert.ToInt32(reader["RoleId"]),
                            Username = reader["Username"].ToString(),
                            Password = reader["Password"].ToString(),
                            Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                            Role = new Role
                            {
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                RoleName = reader["RoleName"].ToString()
                            }
                        };
                        staffList.Add(staff);
                    }
                }
                con.Close();
            }
            return staffList;
        }

        #endregion
    }
}
