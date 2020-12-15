using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Cw3.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cw3.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private const string ConnectionStr = "Data Source=db-mssql;Initial Catalog=s16537;Integrated Security=True";

        [HttpGet] //metoda odpowiada tylko na GET
        public IActionResult GetStudents()
        {
            var list = new List<Student>();

            using (var connection = new SqlConnection(ConnectionStr))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "select * from Student";

                connection.Open();
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var st = new Student();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();

                    list.Add(st);
                }
            }


            return Ok(list);
        }

        [HttpGet("{id}")]
        public IActionResult GetEnrollmentForStudent(string id)
        {
            var list = new List<Enrollment>();

            using (var connection = new SqlConnection(ConnectionStr))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "SELECT E.IdEnrollment, Semester, IdStudy, StartDate FROM Enrollment AS E JOIN" +
                    " STUDENT ON E.IdEnrollment = Student.IdEnrollment WHERE Student.IndexNumber=@id";
                cmd.Parameters.AddWithValue("id", id);

                connection.Open();
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var en = new Enrollment();
                    en.IdEnrollment = dr["IdEnrollment"].ToString();
                    en.Semester = dr["Semester"].ToString();
                    en.IdStudy = dr["IdStudy"].ToString();
                    en.StartDate = dr["StartDate"].ToString();

                    list.Add(en);
                }
            }


            return Ok(list);
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            //.. add to db
            //generate index number
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut("{id}")]
        public IActionResult PutStudent(Student student, int id)
        {
            //.. add to db
            //generate index number
            
            return Ok("Aktualizacja zakonczona.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(Student student, int id)
        {
            //.. delete from db
            return Ok("Usuwanie zakonczone.");
        }

    }
}
