using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.SCRIPTS
{
    public  class DependeciesForExecutePython
    {
        public async Task<(string pythonPath, string scriptPath)> findScriptsForPython()
        {
            string scriptPath = null;
            string projectDirectory;
            string pythonPath = FindPythonPath();  // Call the function here

            if (pythonPath == null)
            {
                // Handle the case where Python path couldn't be found
                //return "Python path not found";
            }            

            if (OperatingSystem.IsMacCatalyst())
            {
                projectDirectory = "/Users/n/Desktop/Genesis5/MauiApp2";
                scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");
            }
            else if (System.OperatingSystem.IsWindows())
            {
                //paths for Windows
                projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\"));
                projectDirectory = projectDirectory.TrimEnd('\\');
                scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");
            }
            else
            {
                // Unsupported OS
                //return string.Empty;
            }

            return (pythonPath, scriptPath);


            //return await MainPage.ExecuteScriptAsync(pythonPath, scriptPath);
        }


        public static string FindPythonPath()
        {

            string[] possibleLocations = Array.Empty<string>(); // Initialize to empty array

            if (OperatingSystem.IsWindows())
            {

                possibleLocations = new string[]
                {
                 "C:\\Python39\\",
                 "C:\\Python38\\",
                 "C:\\Python37\\",
                 "C:\\Python36\\",
                 "C:\\Program Files\\Python311\\",
                 "C:\\Program Files\\Python39\\",
                 "C:\\Program Files\\Python38\\",
                 "C:\\Program Files\\Python37\\",
                 "C:\\Program Files\\Python36\\",
                 "C:\\Program Files (x86)\\Python39\\",
                 "C:\\Program Files (x86)\\Python38\\",
                 "C:\\Program Files (x86)\\Python37\\",
                 "C:\\Program Files (x86)\\Python36\\",
                 "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Programs\\Python\\Python39\\",
                 "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Programs\\Python\\Python38\\",
                 "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Programs\\Python\\Python37\\",
                 "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Programs\\Python\\Python36\\"
                };

                foreach (var location in possibleLocations)
                {
                    string path = Path.Combine(location, "python.exe");
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            else if (OperatingSystem.IsMacCatalyst())
            {
                possibleLocations = new string[]
                {
                  "/usr/local/bin/",
                  "/usr/bin/",
                  "/Users/n/anaconda3/bin/",
                  "/Users/" + Environment.UserName + "/anaconda3/bin/",
                };

                foreach (var location in possibleLocations)
                {
                    string path = Path.Combine(location, "python");
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }

            }


            return null;
        }
    }
}
