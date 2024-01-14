using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.SCRIPTS
{
    public class ProcessChunksAndJson
    {
        private readonly StringBuilder jsonBuffer = new StringBuilder();
        
        public List<string> ProcessChunk(string chunk)  // Changed return type to List<string>
        {
            // Separate lines
            var lines = chunk.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            // Append only lines that are potential JSON objects (starts with '{' and ends with '}')
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("{") && line.Trim().EndsWith("}"))
                {
                    //Debug.WriteLine($"Appending chunk: {line}"); // Debug line
                    jsonBuffer.Append(line);
                }
                else
                {
                    Debug.WriteLine($"Ignoring non-JSON content: {line}"); // Debug line
                }
            }

            return ExtractAndProcessJsonObjects();
        }



        private List<string> ExtractAndProcessJsonObjects()
        {
            string bufferContent = jsonBuffer.ToString();
            //Debug.WriteLine($"Buffer content: {bufferContent}");

            List<string> validJsons = new List<string>();

            // Changed this part to search from the start
            int nextObjectEndIndex = bufferContent.IndexOf('}');
            while (nextObjectEndIndex >= 0)
            {
                string singleJsonObject = bufferContent.Substring(0, nextObjectEndIndex + 1);
                bufferContent = bufferContent.Substring(nextObjectEndIndex + 1);

                try
                {
                    string validJson = MakeValidJson(singleJsonObject);
                    validJsons.Add(validJson);
                }
                catch (JsonReaderException ex)
                {
                    Debug.WriteLine($"JSON parsing error: {ex.Message}");
                    Debug.WriteLine($"Problematic JSON: {singleJsonObject}");
                }

                nextObjectEndIndex = bufferContent.IndexOf('}');
            }

            jsonBuffer.Clear();
            jsonBuffer.Append(bufferContent);

            //Debug.WriteLine($"Total JSON objects extracted: {validJsons.Count}");
            return validJsons.Count > 0 ? validJsons : null;
        }

        private string MakeValidJson(string jsonObject)
        {
            //Debug.WriteLine($"Making JSON valid for: {jsonObject}");

            string validJson = jsonObject.Replace("'start': True", "'start': true")
                                         .Replace("'end': True", "'end': true");
            //Debug.WriteLine($"Valid JSON made: {validJson}");
            return validJson;
        }
    }
}

