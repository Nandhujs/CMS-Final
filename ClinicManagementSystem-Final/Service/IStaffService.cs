using ClinicManagementSystem_Final.Models;

public interface IStaffService
{
    IEnumerable<Staff> GetAllStaff();

    int AddStaff(Staff staff);

    void UpdateStaff(Staff staff);

    Staff GetStaffById(int staffId);
    void DisableStaff(int staffId);

    IEnumerable<Staff> GetStaffByMobileNumber(string mobileNumber);


    IEnumerable<Role> GetAllRoles();

    IEnumerable<Specialization> GetAllSpecializations();

    void AddDoctorDetails(int staffId, int? specializationId, decimal? fee);

    void UpdateDoctorDetails(int staffId, int? specializationId, decimal? fee);
}
