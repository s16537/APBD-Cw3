using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cw3.DTOs;
using Cw3.Models;
using Cw3.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cw3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private const string ConnectionStr = "Data Source=db-mssql;Initial Catalog=s16537;Integrated Security=True";
        private IStudentsDbService _service;

        public EnrollmentsController(IStudentsDbService service)
        {
            _service = service;
        }

        [Route("/api/[controller]/promotions")]
        [HttpPost]
        public IActionResult PromoteStudents(PromoteStudentsRequest request) //transakcja jest zawarta w kodzie procedury
        {
            Enrollment enrollment = _service.PromoteStudents(request);
            if (enrollment == null)
            {
                return NotFound();
            }

            return Ok(enrollment);
        }

        [HttpGet]
        public IActionResult EnrollStudent(AddStudentRequest request)
        {
            Enrollment enrollment = _service.EnrollStudent(request);
            
            if(enrollment == null)
            {
                return NotFound();
            }

            return Ok(enrollment);
        }
    }
}
