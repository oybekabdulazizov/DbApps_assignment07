﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project01.DTOs.Requests;
using Project01.Services;

namespace Project01.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentController : ControllerBase
    {

        public readonly IDbService _idbservice;

        public StudentController(IDbService idbservice)
        {
            _idbservice = idbservice;
        }

        [HttpGet]
        [Authorize(Roles = "employee")]
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
        [Authorize(Roles = "employee")]
        public IActionResult GetStudent(string index)
        {
            var result = _idbservice.GetStudent(index);
            if (result == null)
            {
                return NotFound("Student with this index number does not exist!");
            }

            return Ok(result);
        }

        [HttpPost("saltPassword")]
        public IActionResult SaltPassword(LoginRequest request) 
        {
            var result = _idbservice.AddPasswordAndSalt(request);
            if (result == 0)
            {
                return BadRequest("Please enter valid values!");
            }
            else if (result == -1)
            {
                return NotFound("This user does not exist!");
            }
            else 
            {
                return Ok("The password and a salt have been added!");
            }
        }
    }
}
