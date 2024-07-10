using SysProg.Services;
using SysProg.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace SysProg
{
    public class Program
    {
        static ApiService apiService;

        static void Main(string[] args)
        {
            apiService = new ApiService("https://api.spotify.com/v1/search", "BQC8OjxBvd7UWxHlcyRsVxskfTVWQ5XGKTTEgI0Y4W4GMMhNKOJNFil1JLoBDTL5b5kkynagV56M3WDXgRscus3vu-4jxXmGZowzpNX6FRpJV8XZm_Q Bea");

            string basePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string folderPath = Path.Combine(basePath, "fajlovi");

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(o => RequestHandler.HandleRequest(context, folderPath, apiService));
            }
        }

        static bool IsValidParameter(string param)
        {
            return Regex.IsMatch(param, @"^[a-zA-Z0-9 ]+$");
        }
    }
}
