using Cw3.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cw3.Middleware
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;

        public LogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, LogToFileService logging)
        {
            context.Request.EnableBuffering();
            if(context.Request != null)
            {
                string path = context.Request.Path;
                string method = context.Request.Method;
                string queryStr = context.Request.QueryString.ToString();
                string bodyStr = "";

                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }
                logging.SaveLogs(path, method, queryStr, bodyStr);
            }

            if(_next != null) await _next(context);
        }
    }
}
