using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

public class StaffController : Controller
{
    private readonly IStaffService _staffService;
    private readonly IDoctorService _doctorService;  // Add doctor service to fetch doctor details
    private readonly ILogger<StaffController> _logger;

    public StaffController(IStaffService staffService, IDoctorService doctorService, ILogger<StaffController> logger)
    {
        _staffService = staffService;
        _doctorService = doctorService;
        _logger = logger;
    }

    // GET: Staff
    public IActionResult Index()
    {
        var staffList = _staffService.GetAllStaff();
        return View(staffList);
    }

    // GET: Staff/Details/5
    public IActionResult Details(int id)
    {
        var staff = _staffService.GetStaffById(id);
        if (staff == null)
        {
            return NotFound();
        }
        return View(staff);
    }

    // GET: Staff/Create
    public IActionResult Create()
    {
        var model = new StaffViewModel
        {
            Roles = _staffService.GetAllRoles(),
            Specializations = _staffService.GetAllSpecializations(),
            Staff = new Staff()
        };
        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(StaffViewModel model)
    {
        if (ModelState.IsValid)
        {
            var newStaffId = _staffService.AddStaff(model.Staff);
            model.Roles = _staffService.GetAllRoles();
            model.Specializations = _staffService.GetAllSpecializations();
            var roles = model.Roles ?? new List<Role>();
            var doctorRoleId = roles.FirstOrDefault(r => r.RoleName.ToLower() == "doctor")?.RoleId ?? 2;
            if (model.Staff.RoleId == doctorRoleId)
            {
                _staffService.AddDoctorDetails(newStaffId, model.SelectedSpecializationId, model.Fee);
            }

            return RedirectToAction(nameof(Index));
        }

        // Log ModelState errors for diagnosis
        var errors = ModelState
            .Where(kvp => kvp.Value.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value.Errors.Select(e => new { Key = kvp.Key, Error = e.ErrorMessage, Exception = e.Exception?.Message }))
            .ToList();
        _logger.LogWarning("ModelState invalid in Staff/Create: {@Errors}", errors);

        // Ensure lookups are populated before rendering the view
        PopulateLookups(model);
        return View(model);
    }



    // GET: Staff/Edit/5
    public IActionResult Edit(int id)
    {
        var staff = _staffService.GetStaffById(id);
        if (staff == null)
        {
            return NotFound();
        }

        var model = new StaffViewModel
        {
            Staff = staff,
            Roles = _staffService.GetAllRoles(),
            Specializations = _staffService.GetAllSpecializations(),
            SelectedSpecializationId = 0,
            Fee = null
        };

        var doctorRoleId = model.Roles.FirstOrDefault(r => r.RoleName.ToLower() == "doctor")?.RoleId ?? 2;
        if (staff.RoleId == doctorRoleId)
        {
            var doctorDetails = _doctorService.GetDoctorByStaffId(id);
            if (doctorDetails != null)
            {
                model.SelectedSpecializationId = doctorDetails.SpecializationId;
                model.Fee = doctorDetails.Fee;
            }

        }

        return View(model);
    }

    // POST: Staff/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, StaffViewModel model)
    {
        if (id != model.Staff.StaffId)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            _staffService.UpdateStaff(model.Staff);
            model.Roles = _staffService.GetAllRoles();
            var roles = model.Roles ?? new List<Role>();
var doctorRoleId = roles.FirstOrDefault(r => r.RoleName.ToLower() == "doctor")?.RoleId ?? 2;
            if (model.Staff.RoleId == doctorRoleId)
            {
                _staffService.UpdateDoctorDetails(model.Staff.StaffId, model.SelectedSpecializationId, model.Fee);
            }

            return RedirectToAction(nameof(Index));
        }

        model.Roles = _staffService.GetAllRoles();
        model.Specializations = _staffService.GetAllSpecializations();
        return View(model);
    }

    //disable function
    [HttpPost]
    public IActionResult Disable(int id)
    {
        _staffService.DisableStaff(id);
        return RedirectToAction(nameof(Index));
    }


    // GET: Staff/Search
    [HttpGet]
    public IActionResult Search()
    {
        return View();
    }

    // POST: Staff/Search
    [HttpPost]
    public IActionResult Search(int staffId)
    {
        var staff = _staffService.GetStaffById(staffId);

        if (staff == null)
        {
            TempData["ErrorMessage"] = $"No staff found with ID {staffId}";
            return RedirectToAction("Search");
        }

        var model = new StaffViewModel
        {
            Staff = staff,
            Roles = _staffService.GetAllRoles(),
            Specializations = _staffService.GetAllSpecializations()
        };

        return View("SearchResult", model);
    }

    // POST: Staff/UpdateStaff (for search page)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateStaff(StaffViewModel model)
    {
        // Merge missing (not-posted) fields from DB to avoid validation errors for invisible fields
        var existing = _staffService.GetStaffById(model.Staff.StaffId);
        if (existing == null)
        {
            TempData["ErrorMessage"] = "Staff not found.";
            return RedirectToAction("Search");
        }

        // If the form didn't include these fields, copy from DB and remove ModelState entries so validation uses merged values
        if (string.IsNullOrWhiteSpace(model.Staff.Username))
        {
            model.Staff.Username = existing.Username;
            ModelState.Remove("Staff.Username");
        }
        if (string.IsNullOrWhiteSpace(model.Staff.Password))
        {
            model.Staff.Password = existing.Password;
            ModelState.Remove("Staff.Password");
        }

        if (ModelState.IsValid)
        {
            _staffService.UpdateStaff(model.Staff);
            model.Roles = _staffService.GetAllRoles();
            model.Specializations = _staffService.GetAllSpecializations();

            var doctorRoleId = model.Roles.FirstOrDefault(r => r.RoleName.ToLower() == "doctor")?.RoleId ?? 2;
            if (model.Staff.RoleId == doctorRoleId)
            {
                _staffService.UpdateDoctorDetails(model.Staff.StaffId, model.SelectedSpecializationId, model.Fee);
            }

            TempData["SuccessMessage"] = "Staff details updated successfully!";
            return RedirectToAction("Search");
        }

        TempData["ErrorMessage"] = "Update failed. Please try again.";

        var errors = ModelState
            .Where(kvp => kvp.Value.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value.Errors.Select(e => new { Key = kvp.Key, Error = e.ErrorMessage, Exception = e.Exception?.Message }))
            .ToList();
        _logger.LogWarning("ModelState invalid in Staff/UpdateStaff: {@Errors}", errors);

        PopulateLookups(model); // <<< important: repopulate lookups
        return View("SearchResult", model);
    }

    private void PopulateLookups(StaffViewModel model)
    {
        model.Roles = _staffService.GetAllRoles();
        model.Specializations = _staffService.GetAllSpecializations();
    }
    //Staff serach by mobile number
    // GET: Staff/SearchByMobile
    public IActionResult SearchByMobile()
    {
        return View();
    }

    // POST: Staff/SearchByMobile
    [HttpPost]
    public IActionResult SearchByMobile(string mobileNumber)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            TempData["ErrorMessage"] = "Please enter a mobile number.";
            return View();
        }
        var staffList = _staffService.GetStaffByMobileNumber(mobileNumber);
        if (!staffList.Any())
        {
            TempData["ErrorMessage"] = $"No staff found with mobile number {mobileNumber}.";
            return View();
        }
        return View("SearchResultsList", staffList);
    }


}
