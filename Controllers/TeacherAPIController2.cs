using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Assignment_part_1.Models;
using System;
using MySql.Data.MySqlClient;

namespace Assignment_part_1.Controllers
{
    [Route("api/Teacher2")]
    [ApiController]
    public class TeacherAPIController2 : ControllerBase
    {
       private readonly SchoolDbContext _databaseContext = new SchoolDbContext();

       /// <summary>
        /// Fetches a list of teachers along with their courses, with optional filtering by hire date range.
        /// </summary>
        /// <param name="start">Optional start date for filtering hire dates.</param>
        /// <param name="end">Optional end date for filtering hire dates.</param>
        /// <returns>List of teachers with their details and courses.</returns>
        [HttpGet("GetAll")]
        public IEnumerable<Teacher> GetAllTeachers(DateTime? start = null, DateTime? end = null)
        {
            var teachers = new List<Teacher>();

            using (var connection = _databaseContext.AccessDatabase())
            {
                connection.Open();
                var command = connection.CreateCommand();
                var query = "SELECT * FROM teachers ";

                if (start.HasValue && end.HasValue)
                {
                    query += " WHERE hireDate BETWEEN @startDate AND @endDate";
                    command.Parameters.AddWithValue("@startDate", start.Value);
                    command.Parameters.AddWithValue("@endDate", end.Value);
                }

                command.CommandText = query;

                using (var reader = command.ExecuteReader())
                {
                    var teacherMapping = new Dictionary<int, Teacher>();

                    while (reader.Read())
                    {
                        int teacherId = Convert.ToInt32(reader["TeacherId"]);
                        if (!teacherMapping.ContainsKey(teacherId))
                        {
                            teacherMapping[teacherId] = new Teacher
                            {
                                TeacherId = teacherId,
                                TeacherFName = reader["TeacherFName"].ToString(),
                                TeacherLName = reader["TeacherLName"].ToString(),
                                HireDate = Convert.ToDateTime(reader["HireDate"]),
                                Salary = reader["Salary"].ToString(),
                                EmployeeNumber = reader["EmployeeNumber"].ToString(),
                                CourseNames = new List<string>()
                            };
                        }

                    }

                    teachers.AddRange(teacherMapping.Values);
                }
            }

            return teachers;
        }

        /// <summary>
        /// Retrieves details of a single teacher, including the courses they teach.
        /// </summary>
        /// <param name="teacherId">ID of the teacher.</param>
        /// <returns>Details of the teacher.</returns>
        [HttpGet("GetById/{teacherId}")]
        public Teacher GetTeacherById(int teacherId)
        {
            var teacher = new Teacher();

            using (var connection = _databaseContext.AccessDatabase())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT teachers.*, courses.CourseName FROM teachers " +
                    "JOIN courses ON teachers.TeacherId = courses.TeacherId " +
                    "WHERE teachers.TeacherId = @id";
                command.Parameters.AddWithValue("@id", teacherId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (teacher.TeacherId == 0)
                        {
                            teacher.TeacherId = teacherId;
                            teacher.TeacherFName = reader["TeacherFName"].ToString();
                            teacher.TeacherLName = reader["TeacherLName"].ToString();
                            teacher.HireDate = Convert.ToDateTime(reader["HireDate"]);
                            teacher.Salary = reader["Salary"].ToString();
                            teacher.EmployeeNumber = reader["EmployeeNumber"].ToString();
                            teacher.CourseNames = new List<string>();
                        }

                        teacher.CourseNames.Add(reader["CourseName"].ToString());
                    }
                }
            }

            return teacher;
        }

        /// <summary>
        /// Gets a list of courses taught by a specific teacher.
        /// </summary>
        /// <param name="teacherId">ID of the teacher.</param>
        /// <returns>List of course names.</returns>
        [HttpGet("GetCourses/{teacherId}")]
        public IEnumerable<string> GetCoursesByTeacher(int teacherId)
        {
            var courses = new List<string>();

            using (var connection = _databaseContext.AccessDatabase())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT CourseName FROM courses WHERE TeacherId = @id";
                command.Parameters.AddWithValue("@id", teacherId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(reader["CourseName"].ToString());
                    }
                }
            }

            return courses;
        }

        
        
        /// <summary>
        /// Adds a new teacher to the database.
        /// </summary>
        /// <param name="teacher">Teacher object containing details of the new teacher.</param>
        [HttpPost("Add")]
        public IActionResult AddTeacher([FromBody] Teacher teacher)
        {
            using (var connection = _databaseContext.AccessDatabase())
            {
                connection.Open();

                 // Validate input data
                if (teacher == null)
                {
                    return BadRequest("Teacher data is required.");
                }

                if (string.IsNullOrWhiteSpace(teacher.TeacherFName))
                {
                    return BadRequest("Teacher first name is required.");
                }

                if (string.IsNullOrWhiteSpace(teacher.TeacherLName))
                {
                    return BadRequest("Teacher last name is required.");
                }

                if (teacher.HireDate == default(DateTime))
                {
                    return BadRequest("A valid hire date is required.");
                }

                if (string.IsNullOrWhiteSpace(teacher.Salary))
                {
                    return BadRequest("Salary is required.");
                }

               
                MySqlCommand ExistsCheckCommand = connection.CreateCommand();
                ExistsCheckCommand.CommandText = "SELECT * FROM teachers WHERE employeenumber = @employeenumber";
                ExistsCheckCommand.Parameters.AddWithValue("@employeenumber", teacher.EmployeeNumber);

                // Gather Duplicated Result Set of Query into a variable
                using (MySqlDataReader DuplicatedResultSet = ExistsCheckCommand.ExecuteReader())
                {
                    // If there is a record where employeenumber = @employeenumber
                    if (DuplicatedResultSet.Read())
                    {
                      return BadRequest($"Teacher with employee number {teacher.EmployeeNumber} already exists.");
                    }
                }

                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO teachers (TeacherFName, TeacherLName, HireDate, Salary, EmployeeNumber) " +
                                      "VALUES (@TeacherFName, @TeacherLName, @HireDate, @Salary, @EmployeeNumber)";
                command.Parameters.AddWithValue("@TeacherFName", teacher.TeacherFName);
                command.Parameters.AddWithValue("@TeacherLName", teacher.TeacherLName);
                command.Parameters.AddWithValue("@HireDate", teacher.HireDate);
                command.Parameters.AddWithValue("@Salary", teacher.Salary);
                command.Parameters.AddWithValue("@EmployeeNumber", teacher.EmployeeNumber);
                

                try
                {
                    command.ExecuteNonQuery();
                    return Ok("Teacher added successfully.");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        /// <summary>
        /// Deletes a teacher from the database.
        /// </summary>
        /// <param name="teacherId">ID of the teacher to delete.</param>
        [HttpDelete("Delete/{teacherId}")]
        public IActionResult DeleteTeacher(int teacherId)
        {
            using (var connection = _databaseContext.AccessDatabase())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM teachers WHERE TeacherId = @id";
                command.Parameters.AddWithValue("@id", teacherId);

                try
                {
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Ok("Teacher deleted successfully.");
                    }
                    else
                    {
                        return NotFound("Teacher not found.");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
    }

    [HttpPut("Update/{teacherId}")]
    public IActionResult UpdateTeacher(int teacherId, [FromBody] Teacher updatedTeacher)
    {
    if (updatedTeacher == null || teacherId != updatedTeacher.TeacherId)
    {
        return BadRequest(new { Message = "Invalid teacher data or mismatched ID." });
    }

    var connection = _databaseContext.AccessDatabase();
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
        UPDATE teachers 
        SET teacherfname = @TeacherFName,
            teacherlname = @TeacherLName,
            employeenumber = @EmployeeNumber,
            hiredate = @HireDate,
            salary = @Salary
        WHERE teacherid = @TeacherId";

    command.Parameters.AddWithValue("@TeacherFName", updatedTeacher.TeacherFName);
    command.Parameters.AddWithValue("@TeacherLName", updatedTeacher.TeacherLName);
    command.Parameters.AddWithValue("@EmployeeNumber", updatedTeacher.EmployeeNumber);
    command.Parameters.AddWithValue("@HireDate", updatedTeacher.HireDate);
    command.Parameters.AddWithValue("@Salary", updatedTeacher.Salary);
    command.Parameters.AddWithValue("@TeacherId", teacherId);

    try
    {
        int rowsAffected = command.ExecuteNonQuery();
        connection.Close();

        if (rowsAffected > 0)
        {
            return Ok(new { Message = "Teacher updated successfully." });
        }
        else
        {
            return NotFound(new { Message = "Teacher not found." });
        }
    }
    catch (Exception ex)
    {
        connection.Close();
        return BadRequest(new { Message = $"Error updating teacher: {ex.Message}" });
    }
    }

}
}
