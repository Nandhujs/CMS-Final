using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Repository;

public class StaffServiceImpl : IStaffService
{
    private readonly IStaffRepository _staffRepository;

    public StaffServiceImpl(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public IEnumerable<Staff> GetAllStaff()
    {
        return _staffRepository.GetAllStaff();
    }

    public int AddStaff(Staff staff)
    {
        return _staffRepository.AddStaff(staff);
    }

    public void UpdateStaff(Staff staff)
    {
        _staffRepository.UpdateStaff(staff);
    }

    public Staff GetStaffById(int staffId)
    {
        return _staffRepository.GetStaffById(staffId);
    }
    public void DisableStaff(int staffId)
    {
        _staffRepository.DisableStaff(staffId);
    }
    public IEnumerable<Staff> GetStaffByMobileNumber(string mobileNumber)
    {
        return _staffRepository.GetStaffByMobileNumber(mobileNumber);
    }


    public IEnumerable<Role> GetAllRoles()
    {
        return _staffRepository.GetAllRoles();
    }

    public IEnumerable<Specialization> GetAllSpecializations()
    {
        return _staffRepository.GetAllSpecializations();
    }

    public void AddDoctorDetails(int staffId, int? specializationId, decimal? fee)
    {
        _staffRepository.AddDoctorDetails(staffId, specializationId, fee);
    }

    public void UpdateDoctorDetails(int staffId, int? specializationId, decimal? fee)
    {
        _staffRepository.UpdateDoctorDetails(staffId, specializationId, fee);
    }
}
