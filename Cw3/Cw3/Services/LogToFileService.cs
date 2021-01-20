using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw3.Services
{
    public class LogToFileService
    {
        public void SaveLogs(string path, string method, string queryString, string bodyString)
        {
            using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter("StudentsAPI_Logs.txt", true))
            {
                string line = String.Concat("[", DateTime.Now, "]: Path: ", path, ", Method: ", method, ", Query String: ", queryString, ", Body: ", bodyString);
                file.WriteLine(line);
            }
        }
    }
}
