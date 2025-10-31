using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Repository;

namespace ClinicManagementSystem_Final.Service
{
    public class UserServiceImpl : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserServiceImpl(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Staff AuthenticateUser(string email, string password)
        {
            // Calls repository to check credentials in DB
            return _userRepository.GetUserByEmailAndPassword(email, password);
        }
    }
}
