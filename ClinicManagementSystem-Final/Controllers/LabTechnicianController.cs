using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ClinicManagementSystem_Final.Service;
using ClinicManagementSystem_Final.Models;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ClinicManagementSystem_Final.Controllers
{
    public class LabTechnicianController : Controller
    {
        private readonly ILabTechnicianService _labTechnicianService;

        public LabTechnicianController(ILabTechnicianService labTechnicianService)
        {
            _labTechnicianService = labTechnicianService;
        }

        public async Task<IActionResult> Index()
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
                return View(model);
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
                return RedirectToAction("Index");
            }

            return View(labResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTestResult(LabResultViewModel model)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== UpdateTestResult POST called ===");
                System.Diagnostics.Debug.WriteLine($"Received model: TestId={model.TestId}, PatientId={model.PatientId}, ActualValue={model.ActualValue}");
                System.Diagnostics.Debug.WriteLine($"Model properties: TestName={model.TestName}, PatientName={model.PatientName}");
                System.Diagnostics.Debug.WriteLine($"NormalRange={model.NormalRange}, Remarks={model.Remarks}");
                
                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("ModelState is INVALID");
                    foreach (var error in ModelState)
                    {
                        if (error.Value.Errors.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.Key} - {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                    
                    // Add error message for user
                    TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
                    return View("EditTestResult", model);
                }
                
                System.Diagnostics.Debug.WriteLine("ModelState is VALID");

                // Get technician ID from session (you'll need to implement this)
                var technicianId = 4; // Hardcoded for now - replace with actual logged-in user ID

                System.Diagnostics.Debug.WriteLine($"Updating test: TestId={model.TestId}, PatientId={model.PatientId}, ActualValue={model.ActualValue}");
                
                var result = await _labTechnicianService.UpdateLabResultAsync(model, technicianId);

                System.Diagnostics.Debug.WriteLine($"Update result: success={result.success}, status={result.status}");

                if (result.success)
                {
                    // Return the report with the result status and color
                    model.ValueStatus = result.status;
                    model.StatusColor = result.statusColor;
                    
                    System.Diagnostics.Debug.WriteLine($"Returning report with status: {model.ValueStatus}, color: {model.StatusColor}");
                    
                    // Return the report instead of redirecting
                    return View("LabReportBill", model);
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update test results. Please try again.";
                    return View("EditTestResult", model);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in UpdateTestResult: {ex.Message}");
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View("EditTestResult", model);
            }
        }

        public IActionResult ContinueToNext()
        {
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> GenerateReport(int patientId)
        {
            var report = await _labTechnicianService.GenerateLabReportAsync(patientId);
            return View("LabReport", report);
        }

        public async Task<IActionResult> DownloadReport(int patientId)
        {
            var report = await _labTechnicianService.GenerateLabReportAsync(patientId);

            // Generate PDF report (you'll need to implement PDF generation)
            // For now, return the view that can be printed as PDF
            return View("LabReportPrint", report);
        }

        public async Task<IActionResult> ViewCompletedTests(int patientId)
        {
            var tests = await _labTechnicianService.GetPatientPendingTestsAsync(patientId);
            return View("CompletedTests", tests);
        }
    }
}

