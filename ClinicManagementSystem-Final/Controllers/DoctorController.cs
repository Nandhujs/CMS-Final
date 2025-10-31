using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ClinicManagementSystem_Final.Controllers
{
    public class DoctorController : Controller
    {
        // Field
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // GET: DoctorController
        public ActionResult Index()
        {
            int? staffId = HttpContext.Session.GetInt32("StaffId");
            if (staffId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int? doctorId = _doctorService.GetDoctorIdByStaffId(staffId.Value);
            if (doctorId == null)
            {
                // No doctor record mapped for this staff - handle appropriately.
                return View(new List<DoctorDashboardViewModel>());
            }

            DateTime today = DateTime.Today;
            List<DoctorDashboardViewModel> appointments = _doctorService.GetAppointments(doctorId.Value, today);

            return View(appointments);
        }

        // GET: open consultation view by appointment id
        [HttpGet]
        public ActionResult Consult(int appointmentId)
        {
            if (appointmentId <= 0) return RedirectToAction(nameof(Index));

            int? staffId = HttpContext.Session.GetInt32("StaffId");
            if (staffId == null) return RedirectToAction("Index", "Login");

            var model = _doctorService.GetConsultationByAppointment(appointmentId);
            if (model == null || model.AppointmentId == 0)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            // Redirect to PatientDetails and open the modal there.
            TempData["OpenConsultModal"] = "1";
            return RedirectToAction("PatientDetails", new { mmr = model.MMRNo, appointmentId = appointmentId });
        }

        // POST: Start consultation (save to DB using stored-proc)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StartConsultation(ConsultationCreateViewModel model)
        {
            if (model == null) return RedirectToAction(nameof(Index));

            int? staffId = HttpContext.Session.GetInt32("StaffId");
            if (staffId == null) return RedirectToAction("Index", "Login");

            int? doctorId = _doctorService.GetDoctorIdByStaffId(staffId.Value);
            if (doctorId == null)
            {
                TempData["Error"] = "Doctor record not found for current user.";
                return RedirectToAction(nameof(Index));
            }

            // call service to save
            int status = _doctorService.SaveConsultation(model.AppointmentId, model.PatientId, doctorId.Value,
                model.Symptoms, model.Diagnosis, model.PrescribedMedicine, model.PrescribedLab, model.DoctorNotes);

            switch (status)
            {
                case 0:
                    // success - appointment status already updated by stored proc, but ensure consistency if required
                    TempData["Success"] = "Consultation saved.";
                    return RedirectToAction(nameof(Index));
                case 1:
                    // appointment mismatch / not found
                    ModelState.AddModelError(string.Empty, "Appointment mismatch or not found.");
                    break;
                default:
                    ModelState.AddModelError(string.Empty, "An error occurred saving consultation.");
                    break;
            }

            // On error, re-display consultation view with latest appointment data
            var vm = _doctorService.GetConsultationByAppointment(model.AppointmentId);
            return View("Consultation", vm);
        }

        // GET: show patient details + history + lab results
        [HttpGet]
        public ActionResult PatientDetails(string mmr, int? appointmentId = null)
        {
            if (string.IsNullOrWhiteSpace(mmr))
            {
                return RedirectToAction(nameof(Index));
            }

            int? staffId = HttpContext.Session.GetInt32("StaffId");
            if (staffId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var model = _doctorService.GetPatientByMMR(mmr);
            if (model == null || model.PatientId == 0)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }

            // provide medicines and lab tests for the view
            ViewBag.AvailableMedicines = _doctorService.GetAvailableMedicines();
            ViewBag.AvailableLabTests = _doctorService.GetAllLabTests();

            // expose appointmentId so the modal form can submit against the correct appointment
            ViewBag.AppointmentId = appointmentId ?? 0;

            return View(model);
        }

        // ... other actions unchanged ...
    }
}
