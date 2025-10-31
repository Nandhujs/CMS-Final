using ClinicManagementSystem_Final.Models;
using System.Data.SqlClient;

namespace ClinicManagementSystem_Final.Repository
{
    public class UserRepositoryImpl : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepositoryImpl(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MVCConnectionString");
        }

        public Staff GetUserByEmailAndPassword(string email, string password)
        {
            Staff staff = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query =
                    "SELECT * FROM Staff WHERE Username  = @Username  AND Password = @Password";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Username", email);
                cmd.Parameters.AddWithValue("@Password", password); // plain text for now

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    staff = new Staff
                    {
                        StaffId = (int)reader["StaffId"],
                        Name = reader["Name"].ToString(),
                        Username = reader["Username"].ToString(),
                        RoleId = (int)reader["RoleId"],
                        // include other fields if needed
                    };
                }
            }

            return staff; // returns null if not found
        }
    }
}
