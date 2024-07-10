using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SysProg.Utils
{
    public static class FileUtil
    {
        public static void WriteResultsToFile(string folderPath, List<JObject> results, List<string> queries, List<string> types)
        {
            string fileName = $"Results_{string.Join("_", queries)}_{string.Join("_", types)}.json";
            string filePath = Path.Combine(folderPath, fileName);

            JArray resultArray = JArray.FromObject(results);
            File.WriteAllText(filePath, resultArray.ToString());
        }
    }
}
