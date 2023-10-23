using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MauiApp2
{
    public static class TrimMemoryCS
    {
        public static async Task TrimMemoryFile()
        {
            string FilePath = "C:\\Users\\thega\\source\\repos\\MauiApp2\\MauiApp2\\pMEMORY\\chat_history.txt";
            int MaxCharacters = 5000;

            long characterCount;

            do
            {
                string fileContent = await File.ReadAllTextAsync(FilePath);
                characterCount = fileContent.Length;
                Debug.WriteLine("characterCount: " + characterCount);

                if (characterCount >= MaxCharacters)
                {
                    List<Entry> entries;
                    try
                    {
                        entries = JsonConvert.DeserializeObject<List<Entry>>(fileContent);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error deserializing: " + ex.Message);
                        return;
                    }

                    if (entries != null && entries.Count > 0)
                    {
                        entries.RemoveAt(0); // Remove the oldest entry

                        string updatedContent;
                        try
                        {
                            JsonSerializerSettings settings = new JsonSerializerSettings
                            {
                                Formatting = Formatting.Indented, // Make JSON output indented
                                NullValueHandling = NullValueHandling.Ignore  // Ignore null values
                            };
                            updatedContent = JsonConvert.SerializeObject(entries, settings);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error serializing: " + ex.Message);
                            return;
                        }

                        await File.WriteAllTextAsync(FilePath, updatedContent);
                    }
                    else
                    {
                        Console.WriteLine("No more entries to remove.");
                        break;
                    }
                }
            } while (characterCount >= MaxCharacters);

            Console.WriteLine("Old entries have been removed and the file has been updated.");
        }


        public class Entry
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("language")]
            public string Language { get; set; }

            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("start_of_message")]
            public bool? StartOfMessage { get; set; }

            [JsonProperty("end_of_execution")]
            public bool? EndOfExecution { get; set; }

            [JsonProperty("start_of_code")]
            public bool? StartOfCode { get; set; }

            [JsonProperty("output")]
            public string Output { get; set; }

            [JsonProperty("executing")]
            public object Executing { get; set; }

            [JsonProperty("active_line")]
            public object ActiveLine { get; set; }
        }
    }
}