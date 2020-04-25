using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project01.DTOs.Requests
{
    public class PromotionRequest
    {
        [Required]
        public string Studies { get; set; }

        [Required]
        public int Semester { get; set; }
    }
}
