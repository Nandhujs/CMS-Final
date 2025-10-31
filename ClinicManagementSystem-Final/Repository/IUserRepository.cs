using ClinicManagementSystem_Final.Models;

namespace ClinicManagementSystem_Final.Repository
{
    public interface IUserRepository
    {
        Staff GetUserByEmailAndPassword(string email, string password);
    }
}
