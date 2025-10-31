using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Service;
using Microsoft.AspNetCore.Mvc;

#region
namespace ClinicManagementSystem_Final.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;

        public LoginController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet]
        public IActionResult Index(string loggedout = null)
        {
            // Prevent caching of the login page so back button won't show stale authenticated content
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            ViewBag.LoggedOut = !string.IsNullOrEmpty(loggedout);
            return View();
        }

      

        #region Add
        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userService.AuthenticateUser(model.UserName, model.Password);

                if (user != null)
                {
                    // Set session
                    HttpContext.Session.SetInt32("StaffId", user.StaffId);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetInt32("RoleId", user.RoleId);

                    // Redirect based on role
                    switch (user.RoleId)
                    {
                        case 1: // Admin
                            return RedirectToAction("Index", "Staff");

                        case 2: // Doctor
                            return RedirectToAction("Index", "Doctor");

                        case 3: // Receptionist
                            return RedirectToAction("Index", "Receptionist");

                        case 4: // Pharmacist
                            return RedirectToAction("Index", "Pharmacist");

                        case 5: // Lab Technician
                            return RedirectToAction("Index", "LabTechnician");

                        default:
                            return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ViewBag.Error = "Invalid username or password";
                    return View();
                }
            }

            return View(model);
        }

        // POST logout - clears session and prevents caching; redirects to login with loggedout flag
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.Clear();

            // Prevent caching so browser back won't show authenticated pages
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Redirect to login and indicate we came from logout
            return RedirectToAction(nameof(Index), new { loggedout = "1" });
        }
        #endregion
    }
}
#endregion