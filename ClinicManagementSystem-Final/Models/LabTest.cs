using System;

namespace ClinicManagementSystem_Final.Models
{
    public class LabTest
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public decimal Price { get; set; }
        public string SampleType { get; set; }
        public string NormalRange { get; set; }
        public string Duration { get; set; }
    }
}


