using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Project01.DTOs.Requests;
using Project01.DTOs.Responses;
using Project01.Models;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;

namespace Project01.Services
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
        public Student GetStudent(string index);
        public EnrollmentResponse EnrollStudent(EnrollmentRequest request);
        public PromotionResponse PromoteStudents(PromotionRequest request);
        public LoginResponse Login(LoginRequest request);
        // public string AddSalt(LoginRequest request);

        public void SaveToken(string login, string token);
        public string ValidateToken(string token);
    }
}
