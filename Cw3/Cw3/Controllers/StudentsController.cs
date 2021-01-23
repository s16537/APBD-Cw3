using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cw3.DTOs.Requests;
using Cw3.Models;
using Cw3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cw3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private IStudentsDbService _service;
        private IConfiguration Configuration;

        public StudentsController(IStudentsDbService service, IConfiguration configuration)
        {
            _service = service;
            Configuration = configuration;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult GetStudents()
        {
            var list = _service.GetStudents();

            return Ok(list);
        }

        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            //autoryzacja przy użyciu haseł w bazie danych
            //bool loginStatus = _service.VerifyLogin(request);

            //autoryzacja z wykorzystaniem zewnętrznego kontenera
            //wykorzystano sekrety
            bool loginStatus = request.Password.Equals(Configuration["s000"]);
            if (!loginStatus)
            {
                return Unauthorized("Niepoprawne dane logowania");
            }

            var jwtToken = CreateTokenStudents();
            var refreshTkn = Guid.NewGuid().ToString();
            _service.SaveRefreshToken(refreshTkn, "student");

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
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
                token = new JwtSecurityTokenHandler().WriteToken(CreateTokenStudents()) //zwracamy nowy token
            });
        }

        private JwtSecurityToken CreateTokenStudents()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Andrzej123"),
                new Claim(ClaimTypes.Role, "admin"),
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
