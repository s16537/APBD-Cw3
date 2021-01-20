using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Cw3.Models;
using Cw3.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cw3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private IStudentsDbService _service;

        public StudentsController(IStudentsDbService service)
        {
            _service = service;
        }

        [HttpGet] //metoda odpowiada tylko na GET
        public IActionResult GetStudents()
        {
            var list = _service.GetStudents();

            return Ok(list);
        }

    }
}
