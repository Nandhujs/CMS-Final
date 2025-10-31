using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Service;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ClinicManagementSystem_Final.Controllers
{
    public class ReceptionistController : Controller
    {
        private readonly IReceptionistService _receptionistService;

        public ReceptionistController(IReceptionistService receptionistService)
        {
            _receptionistService = receptionistService;
        }

        // ---------------- Dashboard ----------------
        public IActionResult Index()
        {
            return View(); // Loads Views/Receptionist/Index.cshtml
        }

        // ---------------- Patient Management ----------------

        // Search by MMR No or Phone No (combined)
        public IActionResult Patients(string mmrNo, string phone)
        {
            List<Patient> patients;

            if (!string.IsNullOrEmpty(mmrNo))
                patients = _receptionistService.SearchPatientsByMMRNo(mmrNo.Trim());
            else if (!string.IsNullOrEmpty(phone))
                patients = _receptionistService.SearchPatientsByPhone(phone.Trim());
            else
                patients = _receptionistService.GetAllPatients();

            return View(patients);
        }

        // Patient details
        public IActionResult PatientDetails(int id)
        {
            var patient = _receptionistService.GetPatientById(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // Add new patient (GET)
        public IActionResult AddPatient()
        {
            return View();
        }

        // Add new patient (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPatient(Patient patient)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AddPatient called with: Name={patient?.PatientName}, Gender={patient?.Gender}, DOB={patient?.DOB}");
                
                if (ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("ModelState is valid, calling service");
                    _receptionistService.AddPatient(patient);
                    TempData["SuccessMessage"] = "Patient added successfully!";
                    System.Diagnostics.Debug.WriteLine("Patient added successfully");
                    return RedirectToAction(nameof(Patients));
                }
                else
                {
                    // Log model state errors
                    System.Diagnostics.Debug.WriteLine("ModelState is INVALID");
                    foreach (var error in ModelState)
                    {
                        if (error.Value.Errors.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.Key} - {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                    TempData["ErrorMessage"] = "Please check the form data and try again.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddPatient Controller Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Failed to add patient: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Patients));
        }

        // Update patient (GET)
        public IActionResult EditPatient(int id)
        {
            var patient = _receptionistService.GetPatientById(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // Update patient (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPatient(Patient patient)
        {
            if (ModelState.IsValid)
            {
                _receptionistService.UpdatePatient(patient);
                TempData["SuccessMessage"] = "Patient updated successfully!";
                return RedirectToAction(nameof(Patients));
            }
            TempData["ErrorMessage"] = "Failed to update patient.";
            return RedirectToAction(nameof(Patients));
        }

        // ---------------- Appointments ----------------

        // GET: View today's appointments
        [HttpGet]
        public IActionResult TodayAppointments()
        {
            // Load dropdown data for modal
            ViewBag.Patients = _receptionistService.GetAllPatients();
            ViewBag.Doctors = _receptionistService.GetAllDoctors();

            // Load today's appointments
            var appointments = _receptionistService.GetTodayAppointments();
            return View(appointments);
        }

        // POST: Book new appointment from modal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookAppointment(int patientId, int doctorId, DateTime appointmentDate)
        {
            System.Diagnostics.Debug.WriteLine($"BookAppointment POST called: PatientId={patientId}, DoctorId={doctorId}, AppointmentDate={appointmentDate}");
            
            ViewBag.Patients = _receptionistService.GetAllPatients();
            ViewBag.Doctors = _receptionistService.GetAllDoctors();

            // Validation
            if (patientId <= 0 || doctorId <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"Validation failed: PatientId={patientId}, DoctorId={doctorId}");
                ModelState.AddModelError("", "Invalid patient or doctor selection.");
            }

            if (appointmentDate < DateTime.Now)
            {
                System.Diagnostics.Debug.WriteLine($"Validation failed: AppointmentDate={appointmentDate} is in the past");
                ModelState.AddModelError("", "Appointment time cannot be in the past.");
            }

            if (appointmentDate.Minute % 15 != 0)
            {
                System.Diagnostics.Debug.WriteLine($"Validation failed: AppointmentDate={appointmentDate} minute={appointmentDate.Minute} not divisible by 15");
                ModelState.AddModelError("", "Appointments must be in 15-minute intervals.");
            }

            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (ModelState.IsValid)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Calling service to book appointment");
                    var (appointmentId, token) = _receptionistService.BookAppointment(patientId, doctorId, appointmentDate);
                    
                    System.Diagnostics.Debug.WriteLine($"Service returned: AppointmentId={appointmentId}, Token={token}");
                    
                   if (token > 0 && appointmentId > 0)
                   {
                       System.Diagnostics.Debug.WriteLine("Appointment booked successfully, redirecting to bill view");
                       TempData["SuccessMessage"] = $"Appointment booked successfully. Token Number: {token}";
                       return RedirectToAction("AppointmentBill", new { appointmentId = appointmentId });
                   }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Service returned failure - no appointment created");
                        TempData["ErrorMessage"] = "Failed to book appointment. Please check if patient and doctor exist.";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Booking Error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                    TempData["ErrorMessage"] = $"An error occurred while booking: {ex.Message}";
                }
                
                return RedirectToAction(nameof(TodayAppointments));
            }

            // If validation fails, reload today's appointments
            System.Diagnostics.Debug.WriteLine("ModelState validation failed, reloading appointments");
            var allAppointments = _receptionistService.GetTodayAppointments();
            TempData["ErrorMessage"] = "Failed to book appointment.";
            return View("TodayAppointments", allAppointments);
        }

        // GET: Show appointment bill
        public IActionResult AppointmentBill(int appointmentId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AppointmentBill called with AppointmentId: {appointmentId}");
                
                var bill = _receptionistService.GetAppointmentBill(appointmentId);
                if (bill == null)
                {
                    System.Diagnostics.Debug.WriteLine("Bill data not found for appointment ID: " + appointmentId);
                    TempData["ErrorMessage"] = "Bill data not found.";
                    return RedirectToAction(nameof(TodayAppointments));
                }
                
                System.Diagnostics.Debug.WriteLine($"Bill generated successfully for appointment {appointmentId}");
                return View(bill);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AppointmentBill: {ex.Message}");
                TempData["ErrorMessage"] = "Error generating bill: " + ex.Message;
                return RedirectToAction(nameof(TodayAppointments));
            }
        }

        // Delete Patient (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePatient(int id)
        {
            _receptionistService.DeletePatient(id);
            TempData["SuccessMessage"] = "Patient deleted successfully!";
            return RedirectToAction(nameof(Patients));
        }
    }
}
