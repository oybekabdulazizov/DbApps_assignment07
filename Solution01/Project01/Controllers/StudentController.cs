using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project01.Services;

namespace Project01.Controllers
{
    [Route("api/students")]
    [Authorize]
    [ApiController]
    public class StudentController : ControllerBase
    {

        public readonly IDbService _idbservice;

        public StudentController(IDbService idbservice)
        {
            _idbservice = idbservice;
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            var result = _idbservice.GetStudents();
            if (result.Count() < 1)
            {
                return BadRequest("There is no data!");
            }

            return Ok(result);
        }

        [HttpGet("{index}")]
        public IActionResult GetStudent(string index)
        {
            var result = _idbservice.GetStudent(index);
            if (result == null)
            {
                return NotFound("Student with this index number does not exist!");
            }

            return Ok(result);
        }
    }
}
