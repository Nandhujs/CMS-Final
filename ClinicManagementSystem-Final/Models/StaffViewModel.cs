using ClinicManagementSystem_Final.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClinicManagementSystem_Final.Models
{
    public class StaffViewModel
    {
        // Ensure non-null initialization so view can render dropdowns and radio state
        public Staff Staff { get; set; } = new Staff();

        // UI: choose add staff vs add doctor
        public bool IsDoctor { get; set; }

        [RequiredIf(nameof(IsDoctor), true, ErrorMessage = "Specialization is required for doctors")]
        public int? SelectedSpecializationId { get; set; }

        [RequiredIf(nameof(IsDoctor), true, ErrorMessage = "Fee is required for doctors")]
        public decimal? Fee { get; set; }

        // Lookups used by the view - initialize to avoid null references
        public IEnumerable<Role> Roles { get; set; } = new List<Role>();
        public IEnumerable<Specialization> Specializations { get; set; } = new List<Specialization>();
    }
}