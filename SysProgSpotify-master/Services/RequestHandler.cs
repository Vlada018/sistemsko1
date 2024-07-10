using SysProg.Services;
using SysProg.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace SysProg
{
    public static class RequestHandler
    {
        public static void HandleRequest(HttpListenerContext context, string folderPath, ApiService apiService)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var request = context.Request;
                var response = context.Response;

                LogRequest(request);

                string queriesParam = request.QueryString["queries"];
                string typesParam = request.QueryString["types"];

                if (string.IsNullOrEmpty(queriesParam) || string.IsNullOrEmpty(typesParam))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    byte[] errorBytes = Encoding.UTF8.GetBytes("Parametri nisu validni!");
                    response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                    LogResponse(response, "Parametri nisu validni!");
                }
                else
                {
                    var queries = new List<string>(queriesParam.Split(',')).Where(q => !string.IsNullOrWhiteSpace(q) && IsValidParameter(q)).ToList();
                    var types = new List<string>(typesParam.Split(',')).Where(t => !string.IsNullOrWhiteSpace(t) && IsValidParameter(t)).ToList();

                    if (queries.Count == 0 || types.Count == 0)
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        byte[] errorBytes = Encoding.UTF8.GetBytes("Parametri nisu validni! Proverite da parametri nisu specijalni znakovi!");
                        response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                        LogResponse(response, "Parametri nisu validni! Proverite da parametri nisu specijalni znakovi!");
                    }
                    else
                    {
                        var results = apiService.FetchDataForQueries(queries, types);
                        var resultArray = JArray.FromObject(results);

                        bool containsQuery = false;
                        foreach (var result in resultArray)
                        {
                            foreach (var query in queries)
                            {
                                // Check for albums
                                if (types.Contains("album") && result["albums"]?["items"] != null)
                                {
                                    foreach (var item in result["albums"]["items"])
                                    {
                                        if (item["name"].ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                                        {
                                            containsQuery = true;
                                            break;
                                        }
                                    }
                                }
                                // Check for artists
                                if (types.Contains("artist") && result["artists"]?["items"] != null)
                                {
                                    foreach (var item in result["artists"]["items"])
                                    {
                                        if (item["name"].ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                                        {
                                            containsQuery = true;
                                            break;
                                        }
                                    }
                                }
                                // Check for playlists
                                if (types.Contains("playlist") && result["playlists"]?["items"] != null)
                                {
                                    foreach (var item in result["playlists"]["items"])
                                    {
                                        if (item["name"].ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                                        {
                                            containsQuery = true;
                                            break;
                                        }
                                    }
                                }
                                // Check for tracks
                                if (types.Contains("track") && result["tracks"]?["items"] != null)
                                {
                                    foreach (var item in result["tracks"]["items"])
                                    {
                                        if (item["name"].ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                                        {
                                            containsQuery = true;
                                            break;
                                        }
                                    }
                                }
                                // Check for shows
                                if (types.Contains("show") && result["shows"]?["items"] != null)
                                {
                                    foreach (var item in result["shows"]["items"])
                                    {
                                        if (item["name"].ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                                        {
                                            containsQuery = true;
                                            break;
                                        }
                                    }
                                }
                                // Check for episodes
                                if (types.Contains("episode") && result["episodes"]?["items"] != null)
                                {
                                    foreach (var item in result["episodes"]["items"])
                                    {
                                        if (item["name"].ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                                        {
                                            containsQuery = true;
                                            break;
                                        }
                                    }
                                }
                                // Check for audiobooks
                                if (types.Contains("audiobook") && result["audiobooks"]?["items"] != null)
                                {
                                    foreach (var item in result["audiobooks"]["items"])
                                    {
                                        if (item["name"].ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                                        {
                                            containsQuery = true;
                                            break;
                                        }
                                    }
                                }

                                if (containsQuery)
                                {
                                    break;
                                }
                            }
                            if (containsQuery)
                            {
                                break;
                            }
                        }

                        if (containsQuery)
                        {
                            string resultContent = resultArray.ToString();
                            byte[] buffer = Encoding.UTF8.GetBytes(resultContent);

                            response.ContentType = "application/json";
                            response.ContentLength64 = buffer.Length;
                            response.OutputStream.Write(buffer, 0, buffer.Length);

                            FileUtil.WriteResultsToFile(folderPath, results, queries, types);
                            LogResponse(response, "Request processed successfully.");
                        }
                        else
                        {
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            byte[] errorBytes = Encoding.UTF8.GetBytes("No matching results found!");
                            response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                            LogResponse(response, "No matching results found!");
                        }
                    }
                }

                response.OutputStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            stopwatch.Stop();
            Console.WriteLine($"Time taken: {stopwatch.Elapsed}");
        }

        private static bool IsValidParameter(string param)
        {
            return Regex.IsMatch(param, @"^[a-zA-Z0-9 ]+$");
        }

        private static void LogRequest(HttpListenerRequest request)
        {
            Console.WriteLine("Received request:");
            Console.WriteLine($"{request.HttpMethod} {request.Url}");
            Console.WriteLine($"Query: {request.Url.Query}");
        }

        private static void LogResponse(HttpListenerResponse response, string message)
        {
            Console.WriteLine($"Response: {response.StatusCode} - {message}");
        }
    }
}
