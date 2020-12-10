using Cw3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.DAL
{
    public class MockDbService : IDbService
    {
        private static IEnumerable<Student> _students;
        
        static MockDbService()
        {
            _students = new List<Student> 
            { 
                new Student{IdStudent=1, FirstName="Alan", LastName="Walker"},
                new Student{IdStudent=2, FirstName="Robert", LastName="Proctor"},
                new Student{IdStudent=3, FirstName="Marry", LastName="Loop"},
                new Student{IdStudent=4, FirstName="Jane", LastName="Kowalski"}
            };
        }

        public IEnumerable<Student> GetStudents()
        {
            return _students;
        }
    }
}
