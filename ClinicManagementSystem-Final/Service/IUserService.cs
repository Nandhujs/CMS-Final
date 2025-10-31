using ClinicManagementSystem_Final.Models;

namespace ClinicManagementSystem_Final.Service
{
    public interface IUserService
    {
        Staff AuthenticateUser(string email, string password);
    }
}
