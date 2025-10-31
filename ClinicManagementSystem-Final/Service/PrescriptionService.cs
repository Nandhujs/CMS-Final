using ClinicManagementSystem_Final.Models;
using ClinicManagementSystem_Final.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicManagementSystem_Final.Service
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _prescriptionRepository;

        public PrescriptionService(IPrescriptionRepository prescriptionRepository)
        {
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<IEnumerable<Prescription>> GetAllPrescriptionsAsync()
        {
            return await _prescriptionRepository.GetAllPrescriptionsAsync();
        }

        public async Task<Prescription> GetPrescriptionByIdAsync(int prescriptionId)
        {
            return await _prescriptionRepository.GetPrescriptionByIdAsync(prescriptionId);
        }

        public async Task<IEnumerable<Prescription>> GetPendingPrescriptionsAsync()
        {
            return await _prescriptionRepository.GetPendingPrescriptionsAsync();
        }

        public async Task<bool> DispensePrescriptionAsync(int prescriptionId)
        {
            return await _prescriptionRepository.DispensePrescriptionAsync(prescriptionId);
        }

        public async Task<PrescriptionViewModel> GetPrescriptionForBillAsync(int prescriptionId)
        {
            var prescription = await _prescriptionRepository.GetPrescriptionDetailsForBillAsync(prescriptionId);

            if (prescription == null) return null;

            var viewModel = new PrescriptionViewModel
            {
                PrescriptionId = prescription.PrescriptionId,
                AppointmentId = prescription.AppointmentId,
                MMRNo = prescription.MMRNo,
                PatientName = prescription.PatientName,
                DoctorName = prescription.DoctorName,
                Diagnosis = prescription.Diagnosis,
                ConsultationDate = prescription.ConsultationDate,
                Medicines = prescription.Medicines?.Select(m => new PrescribedMedicineViewModel
                {
                    MedicineId = m.MedicineId,
                    MedicineName = m.MedicineName,
                    Dosage = m.Dosage,
                    Duration = m.Duration,
                    Type = m.Type,
                    Frequency = m.Frequency,
                    Quantity = m.Quantity,
                    Price = m.Price
                }).ToList() ?? new List<PrescribedMedicineViewModel>()
            };

            viewModel.TotalAmount = viewModel.Medicines.Sum(m => m.SubTotal);

            return viewModel;
        }

        public async Task<bool> IsPrescriptionDispensed(int prescriptionId)
        {
            return await _prescriptionRepository.IsPrescriptionDispensed(prescriptionId);
        }
    }
}



