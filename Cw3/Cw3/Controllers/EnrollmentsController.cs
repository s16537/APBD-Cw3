using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Cw3.DTOs;
using Cw3.DTOs.Requests;
using Cw3.Models;
using Cw3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cw3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private IStudentsDbService _service;
        private IConfiguration Configuration;

        public EnrollmentsController(IStudentsDbService service, IConfiguration configuration)
        {
            _service = service;
            Configuration = configuration;
        }


        [Authorize(Roles = "employee")]
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

        [Authorize(Roles = "employee")]
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

        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            //autoryzacja przy użyciu haseł w bazie danych
            //bool loginStatus = _service.VerifyLogin(request);

            //autoryzacja z wykorzystaniem zewnętrznego kontenera
            //wykorzystano sekrety
            bool loginStatus = request.Password.Equals(Configuration["s000"]);
            if(!loginStatus)
            {
                return Unauthorized("Niepoprawne dane logowania");
            }

            var token = CreateTokenEmployees();
            var refreshTkn = Guid.NewGuid().ToString();
            _service.SaveRefreshToken(refreshTkn, "employee");

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = refreshTkn
            });
        }

        [Route("/api/[controller]/refresh-token/")]
        [HttpPost]
        public IActionResult RefreshAccessToken(RefreshTokenRequest request)
        {
            bool isValid = _service.VerifyRefreshToken(request.Token);
            if (!isValid)
            {
                return Unauthorized("Niepoprawny refresh token lub token wygasł.");
            }

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(CreateTokenEmployees()) //zwracamy nowy token
            });
        }

        private JwtSecurityToken CreateTokenEmployees()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Pracownik"),
                new Claim(ClaimTypes.Role, "employee"),
                new Claim(ClaimTypes.Role, "student")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
                (
                    issuer: "PJATK",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds
                );

            return token;
        }
    }
}
