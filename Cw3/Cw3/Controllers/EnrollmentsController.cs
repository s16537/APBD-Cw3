using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cw3.DTOs;
using Cw3.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cw3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private const string ConnectionStr = "Data Source=db-mssql;Initial Catalog=s16537;Integrated Security=True";

        [Route("/api/[controller]/promotions")]
        [HttpPost]
        public IActionResult PromoteStudents(PromoteStudentsRequest request) //transakcja jest zawarta w kodzie procedury
        {
            var studies = request.Studies;
            var semester = request.Semester;

            using (var connection = new SqlConnection(ConnectionStr))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "PromoteStudents";
                cmd.Parameters.AddWithValue("@Studies", studies);
                cmd.Parameters.AddWithValue("@Semester", semester);
                //kod błędu jako parametr wychodzący (OUT) procedury
                cmd.Parameters.Add("@ERROR_CODE", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add("@IdEnrollmentNew", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add("@IdStudies", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add("@StartDate", System.Data.SqlDbType.Date).Direction = System.Data.ParameterDirection.Output;


                connection.Open();
                cmd.ExecuteNonQuery();

                int errorCode = Convert.ToInt32(cmd.Parameters["@ERROR_CODE"].Value);
                if(errorCode == 0)
                {
                    Enrollment enr = new Enrollment();
                    enr.IdEnrollment = Convert.ToInt32(cmd.Parameters["@IdEnrollmentNew"].Value);
                    enr.Semester = semester + 1;
                    enr.IdStudy = Convert.ToInt32(cmd.Parameters["@IdStudies"].Value);
                    enr.StartDate = (DateTime) cmd.Parameters["@StartDate"].Value;
                    return Ok(enr);
                }
                else if (errorCode == 1)
                {
                    return NotFound("Studia o podanej nazwie nie istnieją.");
                }
                else if (errorCode == 2)
                {
                    return NotFound("Nie istnieje wpis na studia o podanych parametrach.");
                }
                else
                {
                    return NotFound();
                }
            }
        }

        [HttpGet]
        public IActionResult AddStudent(AddStudentRequest request)
        {
            var st = new Student();
            st.IndexNumber = request.IndexNumber;
            st.FirstName = request.FirstName;
            st.LastName = request.LastName;
            st.BirthDate = DateTime.Parse(request.BirthDate);

            var studies = request.Studies;

            //select * from Studies where Studies.Name = 'Informatyka'
            using (var connection = new SqlConnection(ConnectionStr))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                connection.Open();
                var tran = connection.BeginTransaction();
                cmd.Transaction = tran;

                cmd.CommandText = "select * from Studies where Studies.Name =@name";
                cmd.Parameters.AddWithValue("name", request.Studies);
                var dr = cmd.ExecuteReader();

                if (!dr.Read())
                {
                    tran.Rollback();
                    return NotFound("Studia nie istnieja.");
                }
                else
                {
                    string IdStudy = dr["IdStudy"].ToString();
                    dr.Close();
                    cmd.CommandText = "select * from Enrollment where IdStudy=@idstudy and Semester=@semester";
                    cmd.Parameters.AddWithValue("idstudy", IdStudy);
                    cmd.Parameters.AddWithValue("semester", 1);

                    dr = cmd.ExecuteReader();

                    int EnrollmentId; // wartosc id dla enrollments
                    DateTime StartDate;
                    if (!dr.Read())
                    {
                        //znalezlismy enrollment o podanych parametrach
                        dr.Close();
                        cmd.CommandText = "select max(IdEnrollment) as MaxID from Enrollment";
                        dr = cmd.ExecuteReader();
                        dr.Read();
                        EnrollmentId = ((int)dr["MaxID"]) + 1;
                        dr.Close();
                        cmd.CommandText = "insert into Enrollment values (@nextid, 1, @idstudy, getdate())";
                        cmd.Parameters.AddWithValue("nextid", EnrollmentId);
                        //cmd.Parameters.AddWithValue("idstudy", IdStudy);
                        //dodajemy wpis do enrollments
                        cmd.ExecuteNonQuery();
                        StartDate = DateTime.Now;
                    }
                    else
                    {
                        EnrollmentId = (int) dr["IdEnrollment"];
                        StartDate = DateTime.Parse(dr["StartDate"].ToString());
                        dr.Close();
                    }

                    //sprawdzamy unikalnosc indexu studenta
                    cmd.CommandText = "select * from Student where IndexNumber =@indexno";
                    cmd.Parameters.AddWithValue("indexno", st.IndexNumber);

                    dr = cmd.ExecuteReader();
                    if (!dr.Read()) //nie ma takiego indexu, robimy insert
                    {
                        dr.Close();
                        cmd.CommandText = "insert into Student values (@indexno, @fname, @lname, @bday, @idenrollment)";
                        //cmd.Parameters.AddWithValue("indexno", st.IndexNumber);
                        cmd.Parameters.AddWithValue("fname", st.FirstName);
                        cmd.Parameters.AddWithValue("lname", st.LastName);
                        cmd.Parameters.AddWithValue("bday", st.BirthDate);
                        cmd.Parameters.AddWithValue("idenrollment", EnrollmentId);

                        cmd.ExecuteNonQuery();

                        //wszystko ok - robimy commit
                        tran.Commit();

                        Enrollment enr = new Enrollment();
                        enr.IdEnrollment = EnrollmentId;
                        enr.Semester = 1;
                        enr.IdStudy = int.Parse(IdStudy);
                        enr.StartDate = StartDate;

                        return Ok(enr);
                    }
                    else
                    {
                        //podany index istnieje - blad
                        dr.Close();
                        tran.Rollback();
                        return BadRequest("Index musi byc unikalny.");
                    }
                    
                }
            }
        }
    }
}
