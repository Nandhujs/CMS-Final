using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem_Final.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        public string RoleName { get; set; }
    }

}
