using ClinicManagementSystem_Final.Service;
using ClinicManagementSystem_Final.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClinicManagementSystem_Final.Controllers
{
    public class LabController : Controller
    {
        private readonly ILabTechnicianService _labTechnicianService;

        public LabController(ILabTechnicianService labTechnicianService)
        {
            _labTechnicianService = labTechnicianService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var pendingTests = await _labTechnicianService.GetAllPendingTestsAsync();
            return View(pendingTests);
        }

        public IActionResult SearchPatient()
        {
            var model = new PatientSearchViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SearchPatient(PatientSearchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Dashboard", model);
            }

            var patient = await _labTechnicianService.SearchPatientAsync(model.SearchTerm, model.SearchType);

            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found. Please check the search criteria.";
                return View(model);
            }

            var tests = await _labTechnicianService.GetPatientPendingTestsAsync(patient.PatientId);

            ViewBag.Patient = patient;
            return View("PatientTests", tests);
        }

        public async Task<IActionResult> EditTestResult(int testId, int patientId)
        {
            var labResult = await _labTechnicianService.GetLabTestForUpdateAsync(testId, patientId);

            if (labResult == null)
            {
                TempData["ErrorMessage"] = "Test not found.";
                return RedirectToAction("Dashboard");
            }

            return View("EditTestResult", labResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTestResult(LabResultViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditTestResult", model);
            }

            var technicianId = 4; // Hardcoded for now
            var result = await _labTechnicianService.UpdateLabResultAsync(model, technicianId);

            if (result.success)
            {
                // Return the report with the result status and color
                model.ValueStatus = result.status;
                model.StatusColor = result.statusColor;
                
                // Return the report instead of redirecting
                return View("LabReportBill", model);
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update test results. Please try again.";
                return View("EditTestResult", model);
            }
        }

        public IActionResult ContinueToNext()
        {
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> GenerateReport(int patientId)
        {
            var report = await _labTechnicianService.GenerateLabReportAsync(patientId);
            return View("LabReport", report);
        }

        public async Task<IActionResult> DownloadReport(int patientId)
        {
            var report = await _labTechnicianService.GenerateLabReportAsync(patientId);
            return View("LabReportPrint", report);
        }
    }
}

