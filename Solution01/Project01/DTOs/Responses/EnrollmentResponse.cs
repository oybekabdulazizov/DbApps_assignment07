using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project01.DTOs.Responses
{
    public class EnrollmentResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Studies { get; set; }
        public int Semester { get; set; }
    }
}
