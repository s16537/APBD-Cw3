using Cw3.DTOs;
using Cw3.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.Services
{
    public interface IStudentsDbService
    {
        Enrollment EnrollStudent(AddStudentRequest request);
        Enrollment PromoteStudents(PromoteStudentsRequest request);
        List<Student> GetStudents();
        Student GetStudent(string index);
    }
}
