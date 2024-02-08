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

        public static int MaxCharacters = 4000;

        public static async Task TrimMemoryFile()
        {
            Debug.WriteLine("TrimMemoryFile");

            string FilePath = GetMemoryFilePath();
            if (!File.Exists(FilePath))
            {
                File.WriteAllText(FilePath, string.Empty);
            }

            long character_count;
            string fileContent = await File.ReadAllTextAsync(FilePath);
            character_count = fileContent.Length;

            Preferences.Set("memory_character_count", character_count);
            Debug.WriteLine("Trim Memory count: " + character_count);

            while (character_count >= MaxCharacters)
            {
                // Deserialize the JSON content into a list of entries
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

                // Check if the list has entries and remove the oldest one
                if (entries != null && entries.Count > 0)
                {
                    entries.RemoveAt(0); // Remove the oldest entry

                    // Serialize the updated list back to JSON
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

                    // Write the updated content back to the file
                    await File.WriteAllTextAsync(FilePath, updatedContent);

                    // Read the file again to update the character count
                    fileContent = await File.ReadAllTextAsync(FilePath);
                    character_count = fileContent.Length;

                    // Update the preference with the new character count
                    Preferences.Set("memory_character_count", character_count);
                    Debug.WriteLine("Trim Memory count: " + character_count);
                }
                else
                {
                    Console.WriteLine("No more entries to remove.");
                    break;
                }
            }


            Debug.WriteLine("Old entries have been removed and the file has been updated.");
        }

        private static string GetMemoryFilePath(string filename = "chat_history.txt")
        {
            string appDataPath;

            if (OperatingSystem.IsWindows())
            {
                appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
            else if (OperatingSystem.IsMacCatalyst())
            {
                // For macOS, directly navigating to the 'Library/Application Support' directory
                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                appDataPath = Path.Combine(homePath, "Library", "Application Support");
            }
            else
            {
                throw new NotSupportedException("Unsupported platform");
            }

            // Ensure to use the correct subdirectory as used in your Python function ('aimee/pMEMORY')
            string pMemoryPath = Path.Combine(appDataPath, "aimee", "pMEMORY");

            // Check if the directory exists, create if not
            if (!Directory.Exists(pMemoryPath))
            {
                Directory.CreateDirectory(pMemoryPath);
            }

            // Return the full path to the file
            return Path.Combine(pMemoryPath, filename);
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