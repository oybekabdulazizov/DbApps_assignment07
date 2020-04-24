using Project01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project01.Services
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();

        public Student GetStudent(string index);
    }
}
