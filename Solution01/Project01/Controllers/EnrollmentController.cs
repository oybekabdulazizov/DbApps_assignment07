using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Project01.DTOs.Requests;
using Project01.Services;

namespace Project01.Controllers
{
    [Route("api")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        public readonly IDbService _idbService;

        public EnrollmentController(IDbService idbService)
        {
            _idbService = idbService;
        }

        [HttpPost("enrollStudent")]
        public IActionResult EnrollStudent(EnrollmentRequest request)
        {
            var result = _idbService.EnrollStudent(request);
            if (result == null)
            {
                return BadRequest("An error occurred!");
            }
            return Ok(result);
        }

        [HttpPost("promoteStudents")]
        public IActionResult PromoteStudents(PromotionRequest request) 
        {
            return Ok(_idbService.PromoteStudents(request));
        }

    }
}
