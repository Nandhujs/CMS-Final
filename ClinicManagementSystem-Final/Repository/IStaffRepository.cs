using ClinicManagementSystem_Final.Models;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Repository
{
    public interface IStaffRepository
    {
        IEnumerable<Staff> GetAllStaff();

        int AddStaff(Staff staff);

        void UpdateStaff(Staff staff);

        Staff GetStaffById(int staffId);

        void DisableStaff(int staffId);

        IEnumerable<Staff> GetStaffByMobileNumber(string mobileNumber);


        IEnumerable<Role> GetAllRoles();

        IEnumerable<Specialization> GetAllSpecializations();

        // Use SpecializationId instead of string specialization in method signatures
        void AddDoctorDetails(int staffId, int? specializationId, decimal? fee);
        void UpdateDoctorDetails(int staffId, int? specializationId, decimal? fee);
    }
}
