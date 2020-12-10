using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cw3.DAL;
using Cw3.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cw3.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet] //metoda odpowiada tylko na GET
        public IActionResult GetStudents(string orderBy)
        {
            return Ok(_dbService.GetStudents());
        }

        [HttpGet("{id}")]
        public IActionResult GetStudent(int id)
        {
            if (id == 1)
            {
                return Ok("Ziobro");
            }
            else if (id == 2)
            {
                return Ok("Stonoga");
            }
            else if (id == 3)
            {
                return Ok("Kowalski");
            }

            return NotFound("Nie znaleziono studenta.");
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
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            student.IdStudent = id;
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
