using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem_Final.Models
{
    public class MedicineViewModel
    {
        public int MedicineId { get; set; }

        [Required]
        [StringLength(100)]
        public string MedicineName { get; set; }

        [Required]
        [StringLength(500)]
        public string MedicineDescription { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
