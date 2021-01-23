using Cw3.DTOs;
using Cw3.DTOs.Requests;
using Cw3.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.Services
{
    public class SQLServerDbService : IStudentsDbService
    {
        private const string ConnectionStr = "Data Source=db-mssql;Initial Catalog=s16537;Integrated Security=True";

        public Enrollment EnrollStudent(AddStudentRequest request)
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
                    return null;
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
                        EnrollmentId = (int)dr["IdEnrollment"];
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

                        return enr;
                    }
                    else
                    {
                        //podany index istnieje - blad
                        dr.Close();
                        tran.Rollback();
                        return null;
                    }

                }
            }
        }

        public Student GetStudent(string index)
        {
            var list = new List<Student>();

            using (var connection = new SqlConnection(ConnectionStr))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "select * from Student where IndexNumber=@index";
                cmd.Parameters.AddWithValue("@index", index);

                connection.Open();
                var dr = cmd.ExecuteReader();
                if (!dr.Read()) // nie ma takiego studenta 
                {
                    return null;
                }
                else
                {
                    var st = new Student();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();

                    return st;
                }

            }
        }

        public List<Student> GetStudents()
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


            return list;
        }

        public Enrollment PromoteStudents(PromoteStudentsRequest request) //transakcja jest zawarta w kodzie procedury
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
                if (errorCode == 0)
                {
                    Enrollment enr = new Enrollment();
                    enr.IdEnrollment = Convert.ToInt32(cmd.Parameters["@IdEnrollmentNew"].Value);
                    enr.Semester = semester + 1;
                    enr.IdStudy = Convert.ToInt32(cmd.Parameters["@IdStudies"].Value);
                    enr.StartDate = (DateTime)cmd.Parameters["@StartDate"].Value;
                    return enr;
                }
                else
                {
                    return null;
                }
            }
        }

        public void RevokeRefreshToken(string token)
        {
            using (var connection = new SqlConnection(ConnectionStr))
            using (var cmd = new SqlCommand())
            {
                //zapisujemy refresh token do bazy
                cmd.Connection = connection;
                connection.Open();
                var transaction = connection.BeginTransaction();

                cmd.CommandText = "update RefreshTokens set IsValid = 0 where Token = @token";
                cmd.Parameters.AddWithValue("@token", token);
                cmd.ExecuteNonQuery();
                transaction.Commit();
            }
        }

        public void SaveRefreshToken(string token, string role)
        {
            using (var connection = new SqlConnection(ConnectionStr))
            using (var cmd = new SqlCommand())
            {
                //zapisujemy refresh token do bazy
                cmd.Connection = connection;
                connection.Open();
                var transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                cmd.CommandText = "insert into RefreshTokens values (@token, @role, 1)";
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@role", role);
                cmd.ExecuteNonQuery();
                transaction.Commit();
            }
        }

        public bool VerifyLogin(LoginRequest request)
        {
            var login = request.Login;
            var password = request.Password;

            using (var connection = new SqlConnection(ConnectionStr))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "select * from Student where IndexNumber=@login and Password=@password";
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);

                connection.Open();
                var dr = cmd.ExecuteReader();
                if (!dr.Read())
                {
                    return false; //niepoprawne dane logowania
                }
                else
                {
                    return true;
                }
            }
        }

        public bool VerifyRefreshToken(string token)
        {
            using (var connection = new SqlConnection(ConnectionStr))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "select * from RefreshTokens where Token = @token and IsValid = 1";
                cmd.Parameters.AddWithValue("@token", token);

                connection.Open();
                try
                {
                    var dr = cmd.ExecuteReader();
                    if (!dr.Read())
                    {
                        return false; //nie znaleziono
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }
            }
        }
    }
}
