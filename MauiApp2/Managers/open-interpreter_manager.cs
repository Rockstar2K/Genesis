using System;
using System.Diagnostics;
using System.Text.Json;

namespace aimee.Managers
{
	public class open_interpreter_manager
	{

        public static async Task<bool> IsLibraryUpToDateAsync(string libraryName)
        {

            var httpClient = new HttpClient();
            string url = $"https://pypi.org/pypi/{libraryName}/json";

            try
            {
                // Command to get the locally installed version
                string localVersionCommand = $"-c \"import pkg_resources; print(pkg_resources.get_distribution('{libraryName}').version)\"";
                // Command to get the latest version available on PyPI
                string latestVersionCommand = $"-m pip search {libraryName} --disable-pip-version-check --no-python-version-warning | grep {libraryName}";

                // Execute the command to get the local version
                string localVersion = await ExecutePythonCommandAsync(localVersionCommand);


                // Execute the command to get the latest version
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                var latestVersion = data.GetProperty("info").GetProperty("version").GetString();
                Console.WriteLine($"Latest version of {libraryName}: {latestVersion}");

                Console.WriteLine($"Local Version: {localVersion}");

                // Compare versions
                return localVersion.Trim() == latestVersion.Trim();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"\nException Caught!");
                Console.WriteLine($"Message :{e.Message}");
                return false;
            }
        }

        private static string ExtractVersionFromSearchOutput(string output, string libraryName)
        {
            // Assuming the output format is "{libraryName} (version) - Description"
            // This method needs to be adapted if the format changes
            int startIndex = output.IndexOf(libraryName) + libraryName.Length + 2; // Skip space and parenthesis
            int endIndex = output.IndexOf(")", startIndex);
            return output.Substring(startIndex, endIndex - startIndex);
        }

        public static async Task<bool> UpdateLibraryAsync(string libraryName)
        {
            try
            {
                string command = $"-m pip install --upgrade {libraryName}";

                // Execute the command to update the library
                string result = await ExecutePythonCommandAsync(command);
                Console.WriteLine(result);

                // You might want to check the result to verify if the update was successful
                // This can be as simple as checking if the output contains certain keywords
                // or more complex based on the output format of the pip command
                if (result.Contains("Successfully installed") || result.Contains("Successfully uninstalled"))
                {
                    return true; // Assuming update was successful
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating library: {ex.Message}");
            }

            return false; // Update failed
        }

        private static async Task<string> ExecutePythonCommandAsync(string command)
        {
            string pythonPath = Preferences.Get("Python_Path", "/Library/Frameworks/Python.framework/Versions/3.10/bin/python3");

            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(pythonPath, command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                process.Start();
                string result = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    throw new InvalidOperationException($"Error executing Python command: {error}");
                }

                return result;
            }
        }
    }


}

