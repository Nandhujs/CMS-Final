using ClinicManagementSystem_Final.Service;
using ClinicManagementSystem_Final.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicManagementSystem_Final.Controllers
{
    public class PharmacistController : Controller
    {
        private readonly IMedicineService _medicineService;
        private readonly IPrescriptionService _prescriptionService;

        public PharmacistController(IMedicineService medicineService, IPrescriptionService prescriptionService)
        {
            _medicineService = medicineService;
            _prescriptionService = prescriptionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Medicine Management Actions
        public async Task<IActionResult> Medicines()
        {
            var medicines = await _medicineService.GetAllMedicinesAsync();
            return View(medicines);
        }

        public IActionResult AddMedicine()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicine(MedicineViewModel medicine)
        {
            if (ModelState.IsValid)
            {
                var result = await _medicineService.AddMedicineAsync(medicine);
                if (result)
                {
                    TempData["SuccessMessage"] = "Medicine added successfully!";
                    return RedirectToAction(nameof(Medicines));
                }
                ModelState.AddModelError("", "Error adding medicine. Please try again.");
            }
            return View(medicine);
        }

        public async Task<IActionResult> EditMedicine(int id)
        {
            var medicine = await _medicineService.GetMedicineByIdAsync(id);
            if (medicine == null)
            {
                return NotFound();
            }

            var viewModel = new MedicineViewModel
            {
                MedicineId = medicine.MedicineId,
                MedicineName = medicine.MedicineName,
                MedicineDescription = medicine.MedicineDescription,
                Quantity = medicine.Quantity,
                Price = medicine.Price
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicine(MedicineViewModel medicine)
        {
            if (ModelState.IsValid)
            {
                var result = await _medicineService.UpdateMedicineAsync(medicine);
                if (result)
                {
                    TempData["SuccessMessage"] = "Medicine updated successfully!";
                    return RedirectToAction(nameof(Medicines));
                }
                ModelState.AddModelError("", "Error updating medicine. Please try again.");
            }
            return View(medicine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicine(int id)
        {
            var result = await _medicineService.DeleteMedicineAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Medicine deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting medicine. Please try again.";
            }
            return RedirectToAction(nameof(Medicines));
        }

        [HttpGet]
        public async Task<IActionResult> SearchMedicines(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                var allMedicines = await _medicineService.GetAllMedicinesAsync();
                return View("Medicines", allMedicines);
            }

            var medicines = await _medicineService.SearchMedicinesAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Medicines", medicines);
        }

        // Prescription Management Actions
        public async Task<IActionResult> Prescriptions()
        {
            var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
            return View(prescriptions);
        }

        public async Task<IActionResult> PendingPrescriptions()
        {
            try
            {
                var allPending = await _prescriptionService.GetPendingPrescriptionsAsync();

                // Filter out prescriptions that are currently being processed (dispensed but bill not confirmed)
                var filteredPending = allPending.Where(p =>
                    TempData["DispensedPrescription"] == null ||
                    p.PrescriptionId != (int)TempData["DispensedPrescription"]
                ).ToList();

                return View(filteredPending);
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading pending prescriptions: {ex.Message}";
                return View(new List<Prescription>());
            }
        }

        public async Task<IActionResult> ViewPrescription(int id)
        {
            var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }
            return View(prescription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DispensePrescription(int prescriptionId)
        {
            try
            {
                // 1. First dispense the medicines (check stock, update quantities)
                var dispenseResult = await _prescriptionService.DispensePrescriptionAsync(prescriptionId);

                if (dispenseResult)
                {
                    // 2. Get bill data and show it immediately
                    var billData = await _prescriptionService.GetPrescriptionForBillAsync(prescriptionId);
                    if (billData != null)
                    {
                        // Store prescriptionId in TempData to mark it as "dispensed but bill not printed"
                        TempData["DispensedPrescription"] = prescriptionId;
                        TempData["SuccessMessage"] = "Medicines dispensed successfully! Please print the bill.";
                        return View("GenerateBill", billData); // Show bill immediately without redirect
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Medicines dispensed but could not generate bill.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Error dispensing prescription. Please check stock availability.";
                }
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }
            return RedirectToAction(nameof(PendingPrescriptions));
        }

        public async Task<IActionResult> GenerateBill(int prescriptionId)
        {
            try
            {
                var billData = await _prescriptionService.GetPrescriptionForBillAsync(prescriptionId);
                if (billData == null)
                {
                    TempData["ErrorMessage"] = "Prescription not found!";
                    return RedirectToAction(nameof(PendingPrescriptions));
                }

                // Check if medicines were actually dispensed
                bool isDispensed = await _prescriptionService.IsPrescriptionDispensed(prescriptionId);
                if (!isDispensed)
                {
                    TempData["ErrorMessage"] = "Please dispense medicines first before generating bill!";
                    return RedirectToAction(nameof(PendingPrescriptions));
                }

                return View(billData);
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating bill: {ex.Message}";
                return RedirectToAction(nameof(PendingPrescriptions));
            }
        }

        // New action to mark bill as printed and remove from pending
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkBillPrinted(int prescriptionId)
        {
            // Clear the temporary dispensed prescription marker
            TempData.Remove("DispensedPrescription");
            TempData["SuccessMessage"] = "Bill printed successfully! Prescription completed.";
            return RedirectToAction(nameof(PendingPrescriptions));
        }

        // PDF Download action
        public async Task<IActionResult> DownloadBillPDF(int prescriptionId)
        {
            try
            {
                var billData = await _prescriptionService.GetPrescriptionForBillAsync(prescriptionId);
                if (billData == null)
                {
                    TempData["ErrorMessage"] = "Prescription not found!";
                    return RedirectToAction(nameof(PendingPrescriptions));
                }

                // Generate PDF using a simple HTML to PDF approach
                var htmlContent = GenerateBillHTML(billData);
                
                // For now, we'll use a simple approach - redirect to the bill view with print parameter
                // In a production environment, you'd use a library like iTextSharp or PuppeteerSharp
                TempData["PrintBill"] = true;
                return View("GenerateBill", billData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating PDF: {ex.Message}";
                return RedirectToAction(nameof(PendingPrescriptions));
            }
        }

        private string GenerateBillHTML(PrescriptionViewModel billData)
        {
            // This is a simplified HTML generation for PDF
            // In production, you'd use a proper PDF library
            var html = $@"
                <html>
                <head>
                    <title>Medicine Bill - {billData.PrescriptionId}</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 20px; }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .bill-info {{ margin-bottom: 20px; }}
                        table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
                        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                        th {{ background-color: #f2f2f2; }}
                        .total {{ font-weight: bold; color: #dc3545; font-size: 1.2em; }}
                        .footer {{ margin-top: 30px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h2>Clinic Management System</h2>
                        <h3>Medicine Bill</h3>
                        <p>Bill No: PH-{DateTime.Now:yyyyMMddHHmmss}</p>
                    </div>
                    
                    <div class='bill-info'>
                        <p><strong>Patient Name:</strong> {billData.PatientName}</p>
                        <p><strong>MMR No:</strong> {billData.MMRNo}</p>
                        <p><strong>Doctor:</strong> {billData.DoctorName}</p>
                        <p><strong>Date:</strong> {billData.ConsultationDate:dd-MM-yyyy}</p>
                    </div>
                    
                    <table>
                        <thead>
                            <tr>
                                <th>S.No</th>
                                <th>Medicine Name</th>
                                <th>Quantity</th>
                                <th>Unit Price</th>
                                <th>Subtotal</th>
                            </tr>
                        </thead>
                        <tbody>";
            
            int serialNumber = 1;
            foreach (var medicine in billData.Medicines)
            {
                html += $@"
                            <tr>
                                <td>{serialNumber}</td>
                                <td>{medicine.MedicineName}</td>
                                <td>{medicine.Quantity}</td>
                                <td>₹{medicine.Price:F2}</td>
                                <td>₹{medicine.SubTotal:F2}</td>
                            </tr>";
                serialNumber++;
            }
            
            html += $@"
                        </tbody>
                        <tfoot>
                            <tr>
                                <th colspan='4' style='text-align: right;'>TOTAL AMOUNT:</th>
                                <th class='total'>₹{billData.TotalAmount:F2}</th>
                            </tr>
                        </tfoot>
                    </table>
                    
                    <div class='footer'>
                        <p><strong>Thank you for choosing our pharmacy!</strong></p>
                        <p>For any queries, please contact: +91-XXXXXXXXXX</p>
                        <p><strong>Pharmacist Signature:</strong> _________________</p>
                        <p><strong>Date:</strong> {DateTime.Now:dd-MM-yyyy}</p>
                    </div>
                </body>
                </html>";
            
            return html;
        }

        // Stock Alert Actions
        public async Task<IActionResult> StockAlerts()
        {
            var lowStockMedicines = await _medicineService.GetLowStockMedicinesAsync();
            return View(lowStockMedicines);
        }

        // TEMPORARY METHOD - Add test prescriptions (remove in production)
        [HttpPost]
        public IActionResult AddTestPrescriptions()
        {
            try
            {
                // This would be your method to add test data
                // You'll need to implement this in your service/repository
                TempData["SuccessMessage"] = "Test prescriptions added successfully!";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding test prescriptions: {ex.Message}";
            }

            return RedirectToAction(nameof(PendingPrescriptions));
        }

        // TEMPORARY METHOD - Add test medicines (remove in production)
        [HttpPost]
        public IActionResult AddTestMedicines()
        {
            try
            {
                // This would be your method to add test data
                // You'll need to implement this in your service/repository
                TempData["SuccessMessage"] = "Test medicines added successfully!";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding test medicines: {ex.Message}";
            }

            return RedirectToAction(nameof(Medicines));
        }
    }
}

