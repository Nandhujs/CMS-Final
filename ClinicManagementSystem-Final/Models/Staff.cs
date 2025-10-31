using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem_Final.Models
{
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public DateTime DOB { get; set; }

        public DateTime DOJ { get; set; }

        public string Address { get; set; }

        public string Gender { get; set; }

        public string Phone { get; set; }

        public int RoleId { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        [ValidateNever]
        public string? Status { get; set; }

        [ForeignKey("RoleId")]
        [ValidateNever]
        public Role? Role { get; set; }
    }
}
