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
        private StringBuilder jsonBuffer = new StringBuilder();

        public string ProcessChunk(string chunk)
        {
            jsonBuffer.Append(chunk);
            var validJson = ExtractAndProcessJsonObjects();

            return validJson;
        }

        private string ExtractAndProcessJsonObjects()
        {
            string bufferContent = jsonBuffer.ToString();
            int lastObjectEndIndex = bufferContent.LastIndexOf('}');
            if (lastObjectEndIndex < 0)
            {
                return string.Empty;  // Return empty string if no valid JSON found
            }

            string objectsString = bufferContent.Substring(0, lastObjectEndIndex + 1);
            jsonBuffer = new StringBuilder(bufferContent.Substring(lastObjectEndIndex + 1));

            string[] jsonObjects = objectsString.Split(new[] { '}' }, StringSplitOptions.RemoveEmptyEntries)
                                               .Select(objStr => objStr + "}").ToArray();
            foreach (string jsonObject in jsonObjects)
            {
                try
                {
                    var validJson = MakeValidJson(jsonObject);
                    return validJson;
                }
                catch (JsonReaderException ex)
                {
                    Debug.WriteLine($"JSON parsing error: {ex.Message}");
                    Debug.WriteLine($"Problematic JSON: {jsonObject}");
                }
            }
            return string.Empty;  // Return empty string if loop ends without returning
        }


        private string MakeValidJson(string jsonObject)
        {
            try
            {
                jsonObject = jsonObject.Replace("'start_of_code': True", "'start_of_code': true")
                                       .Replace("'end_of_execution': True", "'end_of_execution': true")
                                       .Replace("'start_of_message': True", "'start_of_message': true")
                                       .Replace("'end_of_message': True", "'end_of_message': true");
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON parsing error: {ex.Message}");
                Debug.WriteLine($"Problematic JSON: {jsonObject}");
            }
            return jsonObject;
        }

    }
}

