using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Project01.DTOs.Requests;
using Project01.DTOs.Responses;
using Project01.Helpers;
using Project01.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
                    command.CommandText = @"SELECT IndexNumber, FirstName, LastName, BirthDate, IdEnrollment, Password 
                                            FROM Student where IndexNumber=@index";
                    command.Parameters.AddWithValue("index", index);
                    connection.Open();

                    var dr = command.ExecuteReader();
                    if(!dr.Read())
                    {
                        return null;
                    }

                    string _password;
                    if (string.IsNullOrEmpty(dr["Password"].ToString()))
                    {
                        _password = "";
                    }
                    else 
                    {
                        _password = dr["Password"].ToString();
                    }
                    return new Student
                    {
                        IndexNumber = dr["IndexNumber"].ToString(),
                        FirstName = dr["FirstName"].ToString(),
                        LastName = dr["LastName"].ToString(),
                        BirthDate = DateTime.Parse(dr["BirthDate"].ToString()),
                        IdENrollment = int.Parse(dr["IdEnrollment"].ToString()),
                        Password = _password
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
                    command.CommandText = @"SELECT IndexNumber, FirstName, LastName, BirthDate, IdEnrollment, Password FROM Student";
                    connection.Open();

                    var dr = command.ExecuteReader();
                    while (dr.Read())
                    {
                        var _password = dr["Password"].ToString();
                        if (string.IsNullOrEmpty(_password))
                        {
                            _password = "";
                        }
                        
                        var student = new Student
                        {
                            IndexNumber = dr["IndexNumber"].ToString(),
                            FirstName = dr["FirstName"].ToString(),
                            LastName = dr["LastName"].ToString(),
                            BirthDate = DateTime.Parse(dr["BirthDate"].ToString()),
                            IdENrollment = int.Parse(dr["IdEnrollment"].ToString()),
                            Password = _password
                        };
                        students.Add(student);
                    }
                } 

                return students;
            }
        }

        public EnrollmentResponse EnrollStudent(EnrollmentRequest request)
        {

            int _semester = 1;
            EnrollmentResponse response;

            using (var connection = new SqlConnection(SqlServerDb.connectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    command.Transaction = transaction;

                    // let's check if all the passed values are valid 
                    if (request.IndexNumber.ToString() == null || request.FirstName.ToString() == null ||
                        request.LastName.ToString() == null || request.BirthDate == null || request.Studies.ToString() == null)
                    {
                        return null;
                    }

                    // let's check if the requested studies exist in the Studies table 
                    command.CommandText = @"SELECT IdStudy FROM Studies WHERE Name=@study;";
                    command.Parameters.AddWithValue("study", request.Studies);

                    var dataReader1 = command.ExecuteReader();
                    if (!dataReader1.Read())
                    {
                        return null;
                    }
                    int _idStudy = int.Parse(dataReader1["IdStudy"].ToString());
                    dataReader1.Close();

                    // for the existing study, let's find an entry with semester=1 
                    DateTime currentDate = DateTime.Now;
                    command.CommandText = @"SELECT MAX(IdEnrollment) AS MaxId FROM Enrollment 
                                        WHERE Semester=1 AND IdStudy=@idStudy;";
                    command.Parameters.AddWithValue("idStudy", _idStudy);

                    var dataReader2 = command.ExecuteReader();
                    int latestEntry = 0;

                    if (dataReader2.Read())
                    {

                        var result = dataReader2["MaxId"].ToString();
                        if (!string.IsNullOrEmpty(result))
                        {
                            latestEntry = int.Parse(result);
                            dataReader2.Close();
                        }
                        else
                        {
                            dataReader2.Close();
                            command.CommandText = @"SELECT MAX(IdEnrollment) AS MaxId FROM Enrollment;";
                            var dataReader3 = command.ExecuteReader();
                            // let's check if there any enrollment exists 
                            if (dataReader3.Read())
                            {

                                var maxId = dataReader3["MaxId"].ToString();
                                if (!string.IsNullOrEmpty(maxId))
                                {
                                    latestEntry = int.Parse(maxId);
                                }

                                latestEntry++;
                                dataReader3.Close();
                                command.CommandText = @"INSERT INTO Enrollment VALUES (@idEnroll, @_semester, @_idStudy, @_startDate)";
                                command.Parameters.AddWithValue("idEnroll", latestEntry);
                                command.Parameters.AddWithValue("_semester", _semester);
                                command.Parameters.AddWithValue("_idStudy", _idStudy);
                                command.Parameters.AddWithValue("_startDate", currentDate);
                                command.ExecuteNonQuery();
                            }
                        }

                    }

                    // here, we check if newly entered index number is assigned to another student. 
                    // If a student already exists with the given index number, we return error
                    // If not, then with that index number, we insert a new student into Students table
                    command.CommandText = @"SELECT FirstName FROM Student WHERE IndexNumber=@idStudent;";
                    command.Parameters.AddWithValue("idStudent", request.IndexNumber);
                    using var dataReader4 = command.ExecuteReader();
                    if (!dataReader4.Read())
                    {
                        dataReader4.Close();
                        command.CommandText =
                            @"INSERT INTO Student VALUES (@id, @name, @surname, CONVERT(DATE, @dob, 103), @idE, @password);";
                        command.Parameters.AddWithValue("@id", request.IndexNumber);
                        command.Parameters.AddWithValue("@name", request.FirstName);
                        command.Parameters.AddWithValue("@surname", request.LastName);
                        command.Parameters.AddWithValue("@dob", request.BirthDate);
                        command.Parameters.AddWithValue("@idE", latestEntry);
                        command.Parameters.AddWithValue("@password", "default");

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        // returning error if the given id is already assigned to another student
                        return null;
                    }

                    transaction.Commit();

                    // Done :)
                    response = new EnrollmentResponse();
                    response.FirstName = request.FirstName;
                    response.LastName = request.LastName;
                    response.Studies = request.Studies;
                    response.Semester = _semester;

                    return response;
                }
            }
        }

        public PromotionResponse PromoteStudents(PromotionRequest request)
        {
            PromotionResponse response;

            using (var connection = new SqlConnection(SqlServerDb.connectionString))
            {
                using (var command = new SqlCommand())
                {

                    command.Connection = connection;

                    command.CommandText = "PromoteStudents";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@studies", request.Studies);
                    command.Parameters.AddWithValue("@semester", request.Semester);
                    connection.Open();
                    command.ExecuteNonQuery();

                    response = new PromotionResponse
                    {
                        Studies = request.Studies,
                        Semester = request.Semester + 1
                    };

                }
            }

            return response;
        }

        // checks if username and password are valid and correct
        public string Login(LoginRequest request)
        {
            string _firstName, _salt;
            using (var connection = new SqlConnection(SqlServerDb.connectionString))
            {
                using (var command = new SqlCommand())
                {

                    if (request.Login == null || request.Login.Trim() == null ||
                                request.Password.Trim() == null || request.Password.Trim() == null)
                    {
                        return "0"; // Bad Request 
                    }

                    command.Connection = connection;
                    command.CommandText = @"SELECT * FROM Student WHERE IndexNumber=@id;";
                    command.Parameters.AddWithValue("id", request.Login);
                    connection.Open();
                    var dr = command.ExecuteReader();
                    if (!dr.Read())
                    {
                        return "-1"; // Student Not Found
                    }
                    _firstName = dr["FirstName"].ToString();
                    dr.Close();


                    command.CommandText = @"SELECT * FROM Salt WHERE IdNumber=@login;";
                    command.Parameters.AddWithValue("login", request.Login);

                    var dr2 = command.ExecuteReader();
                    if (!dr2.Read())
                    {
                        return "-2"; // Salt Not Found
                    }
                    _salt = dr2["SaltValue"].ToString();
                    dr2.Close();

                    var hashedPassword = Generator.Hash(request.Password, _salt);
                    command.CommandText = @"SELECT IndexNumber, FirstName FROM Student 
                                            WHERE IndexNumber=@indexNumber AND Password=@password;";
                    command.Parameters.AddWithValue("indexNumber", request.Login);
                    command.Parameters.AddWithValue("password", hashedPassword);
                    using (var dr3 = command.ExecuteReader())
                    {
                        if (!dr3.Read())
                        {
                            return "-3"; // Bad Request(Wrong password)
                        }
                    }

                    return _firstName;
                }
            }
        }

        // saves each and every login and refresh-tokens
        public void SaveRefreshToken(string login, string refreshToken)
        {
            using (var connection = new SqlConnection(SqlServerDb.connectionString)) 
            {
                using (var command = new SqlCommand()) 
                {
                    command.Connection = connection;
                    command.CommandText = @"INSERT INTO Token VALUES (@login, @token);";
                    command.Parameters.AddWithValue("login", login);
                    command.Parameters.AddWithValue("token", refreshToken);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // validates and renews refresh-tokens 
        public string ValidateToken(string token) 
        {
            string response;
            using (var connection = new SqlConnection(SqlServerDb.connectionString)) 
            {
                using (var command = new SqlCommand()) 
                {
                    command.Connection = connection;
                    command.CommandText = @"SELECT IndexNumber, Token FROM Token WHERE Token=@token;";
                    command.Parameters.AddWithValue("token", token);
                    connection.Open();
                    using (var dr4 = command.ExecuteReader()) 
                    {
                        if (!dr4.Read()) 
                        {
                            return null;
                        }
                        response = dr4["IndexNumber"].ToString();   
                    }

                    command.CommandText = @"DELETE FROM Token WHERE Token=@oldToken;";
                    command.Parameters.AddWithValue("oldToken", token);
                    command.ExecuteNonQuery();

                    return response;
                }
            }
        }

        // if a salt for a user does not exist, this method adds a salt for a user and hashes the password for the security
        public int AddPasswordAndSalt(LoginRequest request) 
        {

            if (request.Login == null || request.Login.Trim() == null ||
                                request.Password.Trim() == null || request.Password.Trim() == null)
            {
                return 0; // Bad Request 
            }

            string _firstName, _salt, _readyPassword;
            using (var connection = new SqlConnection(SqlServerDb.connectionString))
            {
                using (var command = new SqlCommand())
                {

                    command.Connection = connection;
                    command.CommandText = @"SELECT * FROM Student WHERE IndexNumber=@login;";
                    command.Parameters.AddWithValue("login", request.Login);
                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    command.Transaction = transaction;

                    using (var dr = command.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            return -1; // Not Found 
                        }
                        _firstName = dr["FirstName"].ToString();
                    }

                    command.CommandText = @"SELECT * FROM Salt WHERE IdNumber=@loginn;";
                    command.Parameters.AddWithValue("loginn", request.Login);
                    var dr2 = command.ExecuteReader();
                    if (!dr2.Read())
                    {
                        _salt = Generator.CreateSalt();
                        command.CommandText = @"INSERT INTO Salt VALUES (@id, @name, @salt);";
                        command.Parameters.AddWithValue("id", request.Login);
                        command.Parameters.AddWithValue("name", _firstName);
                        command.Parameters.AddWithValue("salt", _salt);
                        dr2.Close();
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        _salt = dr2["SaltValue"].ToString();
                        dr2.Close();
                    }

                    _readyPassword = Generator.Hash(request.Password, _salt);
                    command.CommandText = @"UPDATE Student SET Password=@pass WHERE IndexNumber=@index;";
                    command.Parameters.AddWithValue("pass", _readyPassword);
                    command.Parameters.AddWithValue("index", request.Login);
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    return 1;
                }
            }
        }

    }
}
