﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Project01.DTOs.Requests;
using Project01.Helpers;
using Project01.Services;

namespace Project01.Controllers
{
    [Route("api")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        public readonly IDbService _idbService;

        protected IConfiguration Configuration;

        public EnrollmentController(IDbService idbService, IConfiguration configuration)
        {
            _idbService = idbService;
            Configuration = configuration;
        }

        [HttpPost("enrollStudent")]
        [Authorize(Roles = "employee")]
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
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(PromotionRequest request)
        {
            return Ok(_idbService.PromoteStudents(request));
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var response = _idbService.Login(request);
            if (response == null) 
            {
                return BadRequest("Invalid login or password!");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.Login),
                new Claim(ClaimTypes.Name, response.FirstName),
                new Claim(ClaimTypes.Role, "employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
             (
                issuer: "Oybek",
                audience: "everyone",
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
             );

            // == we need to save the token here and proceed
            _idbService.SaveToken(response.Login, token.ToString());

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            });

        }

        [HttpPost("refresh-token/{token}")]
        public IActionResult RefreshToken(string token) 
        {
            var validateTokenLogin = _idbService.ValidateToken(token);
            if (validateTokenLogin == null) 
            {
                return NotFound("This token does not exist!");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, validateTokenLogin),
                new Claim(ClaimTypes.Role, "employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var newToken = new JwtSecurityToken
             (
                issuer: "Oybek",
                audience: "everyone",
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
             );

            _idbService.SaveToken(validateTokenLogin, newToken.ToString());

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(newToken),
                refreshToken = Guid.NewGuid()
            });

        }

    }
}
