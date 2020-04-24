using Project01.Helpers;
using Project01.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Project01.Services
{
    public class SqlServerDbService : IDbService
    {
        public Student GetStudent(string index)
        {
            Student student = new Student();
            using (var connection = new SqlConnection(SqlServerDb.connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"SELECT IndexNumber, FirstName, LastName, BirthDate, IdEnrollment FROM Student 
                                            where IndexNumber=@index";
                    command.Parameters.AddWithValue("index", index);
                    connection.Open();

                    var dr = command.ExecuteReader();
                    if(!dr.Read())
                    {
                        return null;
                    }

                    return new Student
                    {
                        IndexNumber = dr["IndexNumber"].ToString(),
                        FirstName = dr["FirstName"].ToString(),
                        LastName = dr["LastName"].ToString(),
                        BirthDate = DateTime.Parse(dr["BirthDate"].ToString()),
                        IdENrollment = int.Parse(dr["IdEnrollment"].ToString())
                    };
                }
            }
        }

        public IEnumerable<Student> GetStudents()
        {
            var students = new List<Student>();
            using (var connection = new SqlConnection(SqlServerDb.connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"SELECT IndexNumber, FirstName, LastName, BirthDate, IdEnrollment FROM Student";
                    connection.Open();

                    var dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        var student = new Student
                        {
                            IndexNumber = dr["IndexNumber"].ToString(),
                            FirstName = dr["FirstName"].ToString(),
                            LastName = dr["LastName"].ToString(),
                            BirthDate = DateTime.Parse(dr["BirthDate"].ToString()),
                            IdENrollment = int.Parse(dr["IdEnrollment"].ToString())
                        };
                        students.Add(student);
                    }
                }

                if (students.Count() < 1)
                {
                    return null;
                }
                return students;
            }
        }
    }
}
