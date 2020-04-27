using System;
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
            using (var connection = new SqlConnection(SqlServerDb.connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"Select 1 FROM Student WHERE IndexNumber=@login;";
                    command.Parameters.AddWithValue("login", request.Login);
                    connection.Open();

                    using (var dr = command.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            return NotFound($"Student with {request.Login} index number does not exist!");
                        }
                    }

                    command.CommandText = @"UPDATE Student SET Password=@password WHERE IndexNumber=@index;";
                    command.Parameters.AddWithValue("@password", request.Password);
                    command.Parameters.AddWithValue("@index", request.Login);
                    command.ExecuteNonQuery();
                }
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.Login),
                new Claim(ClaimTypes.Role, "student")
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

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            });

        }

    }
}
