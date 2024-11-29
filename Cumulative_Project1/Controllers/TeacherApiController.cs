using Cumulative_Project1.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Cumulative_Project1.Controllers
{
    [Route("api/Teacher")]
    [ApiController]
    public class TeacherAPIController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        // Dependency injection of database context
        public TeacherAPIController(SchoolDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of teachers in the system. Optional search key filters teachers by first or last name.
        /// </summary>
        /// <example>
        /// GET: api/Teacher/ListTeachers?SearchKey=John -> [{"TeacherId":1, "TeacherFirstName":"John", "TeacherLastName":"Doe", ...}]
        /// </example>
        /// <returns>
        /// A list of teacher objects
        /// </returns>
        [HttpGet]
        [Route("ListTeachers")]
        public List<Teacher> ListTeachers(string SearchKey = null)
        {
            List<Teacher> Teachers = new List<Teacher>();

            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();

                string query = "SELECT * FROM teachers";
                if (SearchKey != null)
                {
                    query += " WHERE LOWER(teacherfname) LIKE @key OR LOWER(teacherlname) LIKE @key OR LOWER(CONCAT(teacherfname, ' ', teacherlname)) LIKE @key";
                    Command.Parameters.AddWithValue("@key", $"%{SearchKey.ToLower()}%");
                }
                Command.CommandText = query;
                Command.Prepare();

                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    while (ResultSet.Read())
                    {
                        Teacher CurrentTeacher = new Teacher()
                        {
                            TeacherId = Convert.ToInt32(ResultSet["teacherid"]),
                            TeacherFirstName = ResultSet["teacherfname"].ToString(),
                            TeacherLastName = ResultSet["teacherlname"].ToString(),
                            EmployeeNumber = ResultSet["employeenumber"].ToString(),
                            HireDate = Convert.ToDateTime(ResultSet["hiredate"]),
                            Salary = Convert.ToDecimal(ResultSet["salary"])
                        };

                        Teachers.Add(CurrentTeacher);
                    }
                }
            }

            return Teachers;
        }

        /// <summary>
        /// Returns a teacher in the database by their ID
        /// </summary>
        /// <example>
        /// GET: api/Teacher/FindTeacher/1 -> {"TeacherId":1, "TeacherFirstName":"John", "TeacherLastName":"Doe", ...}
        /// </example>
        /// <returns>
        /// A matching teacher object by its ID
        /// </returns>
        [HttpGet]
        [Route("FindTeacher/{id}")]
        public Teacher FindTeacher(int id)
        {
            Teacher SelectedTeacher = new Teacher();

            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();

                Command.CommandText = "SELECT * FROM teachers WHERE teacherid = @id";
                Command.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    while (ResultSet.Read())
                    {
                        SelectedTeacher.TeacherId = Convert.ToInt32(ResultSet["teacherid"]);
                        SelectedTeacher.TeacherFirstName = ResultSet["teacherfname"].ToString();
                        SelectedTeacher.TeacherLastName = ResultSet["teacherlname"].ToString();
                        SelectedTeacher.EmployeeNumber = ResultSet["employeenumber"].ToString();
                        SelectedTeacher.HireDate = Convert.ToDateTime(ResultSet["hiredate"]);
                        SelectedTeacher.Salary = Convert.ToDecimal(ResultSet["salary"]);
                    }
                }
            }

            return SelectedTeacher;
        }

        /// <summary>
        /// Adds a teacher to the database
        /// </summary>
        /// <example>
        /// POST: api/Teacher/AddTeacher
        /// Headers: Content-Type: application/json
        /// Request Body:
        /// {
        ///     "TeacherFirstName": "John",
        ///     "TeacherLastName": "Doe",
        ///     "EmployeeNumber": "T123",
        ///     "HireDate": "2020-01-01",
        ///     "Salary": 60000
        /// }
        /// </example>
        /// <returns>
        /// The inserted Teacher ID
        /// </returns>
        [HttpPost]
        [Route("AddTeacher")]
        public int AddTeacher([FromBody] Teacher TeacherData)
        {
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();

                Command.CommandText = "INSERT INTO teachers (teacherfname, teacherlname, employeenumber, hiredate, salary) " +
                                      "VALUES (@teacherfname, @teacherlname, @employeenumber, @hiredate, @salary)";
                Command.Parameters.AddWithValue("@teacherfname", TeacherData.TeacherFirstName);
                Command.Parameters.AddWithValue("@teacherlname", TeacherData.TeacherLastName);
                Command.Parameters.AddWithValue("@employeenumber", TeacherData.EmployeeNumber);
                Command.Parameters.AddWithValue("@hiredate", TeacherData.HireDate);
                Command.Parameters.AddWithValue("@salary", TeacherData.Salary);

                Command.ExecuteNonQuery();
                return Convert.ToInt32(Command.LastInsertedId);
            }
        }

        /// <summary>
        /// Deletes a teacher from the database
        /// </summary>
        /// <example>
        /// DELETE: api/Teacher/DeleteTeacher/1 -> 1
        /// </example>
        /// <returns>
        /// The number of rows affected
        /// </returns>
        [HttpDelete]
        [Route("DeleteTeacher/{id}")]
        public int DeleteTeacher(int id)
        {
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();

                Command.CommandText = "DELETE FROM teachers WHERE teacherid = @id";
                Command.Parameters.AddWithValue("@id", id);

                return Command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Updates a teacher in the database
        /// </summary>
        /// <example>
        /// PUT: api/Teacher/UpdateTeacher/1
        /// Headers: Content-Type: application/json
        /// Request Body:
        /// {
        ///     "TeacherFirstName": "John",
        ///     "TeacherLastName": "Doe",
        ///     "EmployeeNumber": "T123",
        ///     "HireDate": "2020-01-01",
        ///     "Salary": 65000
        /// }
        /// </example>
        /// <returns>
        /// The updated Teacher object
        /// </returns>
        [HttpPut]
        [Route("UpdateTeacher/{id}")]
        public Teacher UpdateTeacher(int id, [FromBody] Teacher TeacherData)
        {
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();

                Command.CommandText = "UPDATE teachers SET teacherfname = @teacherfname, teacherlname = @teacherlname, " +
                                      "employeenumber = @employeenumber, hiredate = @hiredate, salary = @salary " +
                                      "WHERE teacherid = @id";
                Command.Parameters.AddWithValue("@teacherfname", TeacherData.TeacherFirstName);
                Command.Parameters.AddWithValue("@teacherlname", TeacherData.TeacherLastName);
                Command.Parameters.AddWithValue("@employeenumber", TeacherData.EmployeeNumber);
                Command.Parameters.AddWithValue("@hiredate", TeacherData.HireDate);
                Command.Parameters.AddWithValue("@salary", TeacherData.Salary);
                Command.Parameters.AddWithValue("@id", id);

                Command.ExecuteNonQuery();
            }

            return FindTeacher(id);
        }
    }
}
