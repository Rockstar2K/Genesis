using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MauiApp2;

public partial class Welcome_Page : ContentPage
{
    private double progressValue;
    private const string PythonInstallerUrlWindows = "https://www.python.org/ftp/python/3.10.0/python-3.10.0-amd64.exe";
    private const string PythonInstallerUrlMac = "https://www.python.org/ftp/python/3.10.0/python-3.10.0-macos11.pkg";

    private const string PythonInstallerNameWindows = "python-3.10.0-amd64.exe";
    private const string PythonInstallerNameMac = "python-3.10.0-macosx11.pkg";


    public Welcome_Page()
    {
        InitializeComponent();
    }

    private async void Download_Clicked(object sender, EventArgs e)
    {
        subHeader.Text = "We're setting up everything";
        Download_Button.IsVisible = false;
        progressBar.IsVisible = true;
        TermsAndConditions.IsVisible = false;


        StartPythonInstallation();
        //App.Current.MainPage = new NavigationPage(new API_Key_Page());

    }

    private async void StartPythonInstallation()
    {
        try
        {
            await DisplayAlert("Starting", "Starting Python download and installation.", "OK");

            string installerUrl, installerName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                installerUrl = PythonInstallerUrlWindows;
                installerName = PythonInstallerNameWindows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                installerUrl = PythonInstallerUrlMac;
                installerName = PythonInstallerNameMac;
            }
            else
            {
                installerUrl = PythonInstallerUrlMac;
                installerName = PythonInstallerNameMac;
                //  await DisplayAlert("Error", "Unsupported operating system.", "OK");
                //return;
            }

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(installerUrl, HttpCompletionOption.ResponseHeadersRead);

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var fileInfo = new FileInfo(installerName);
                using (var fileStream = fileInfo.OpenWrite())
                {
                    await CopyStreamWithProgress(stream, fileStream, response.Content.Headers.ContentLength ?? -1);
                }
            }
            await DownloadPythonInstaller(installerUrl, installerName);

            await DisplayAlert("Download Complete", "Python installer downloaded successfully.", "OK");

            InstallPython(installerName);

            await DisplayAlert("Notice", "Please complete the Python installation process and then press OK.", "OK");

            string pythonPath = await GetPythonInstallationPath();

            Preferences.Set("Python_Path", pythonPath);

            if (!string.IsNullOrEmpty(pythonPath))
            {
                await DisplayAlert("Python Path", $"Python was installed at: {pythonPath}", "OK");

                //Update pip
                await UpgradePipAsync(pythonPath);

                // After Python installation is complete, install open-interpreter
                await InstallPythonLibrary("open-interpreter");
                progressBar.Progress = 1;
                await DisplayAlert("Installation Complete", "Open Interpreterhas been installed successfully.", "OK");
            }
            else
            {
                // Handle the error or absence of Python path
                await DisplayAlert("Error", "Python installation path not found.", "OK");
            }

            // After Python installation is complete, install open-interpreter
            await InstallPythonLibrary("open-interpreter");
            progressBar.Progress = 1;
            await DisplayAlert("Installation Complete", "Open Interpreter has been installed successfully.", "OK");

            App.Current.MainPage = new NavigationPage(new API_Key_Page());

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}. Try again. If the error persists, let us know at hello@get-aimee.com", "OK");
            progressBar.IsVisible = false;
            Download_Button.IsVisible = true;
        }
    }

    private async Task<string> DownloadPythonInstaller(string installerUrl, string installerName)
    {
        var tempDirectory = Path.GetTempPath();
        var installerPath = Path.Combine(tempDirectory, installerName);

        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(installerUrl, HttpCompletionOption.ResponseHeadersRead);

        using (var stream = await response.Content.ReadAsStreamAsync())
        {
            using (var fileStream = new FileStream(installerPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await CopyStreamWithProgress(stream, fileStream, response.Content.Headers.ContentLength ?? -1);
            }
        }

        return installerPath; // Correctly return the path as a string
    }

    private async Task CopyStreamWithProgress(Stream inputStream, Stream outputStream, long totalLength)
    {
        var buffer = new byte[81920];
        int read;
        long totalRead = 0;
        while ((read = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await outputStream.WriteAsync(buffer, 0, read);
            totalRead += read;
            if (totalLength != -1)
            {
                // Update progress to reach up to 90%
                var progress = 0.9 * ((double)totalRead / totalLength);
                progressBar.Progress = progress;
            }
        }
    }


    private void InstallPython(string installerName)
    {
        var process = new Process();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var installationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "aimee", "Python");
            Directory.CreateDirectory(installationPath); // Ensure the directory exists


            process.StartInfo = new ProcessStartInfo
            {
                FileName = installerName,
                Arguments = $"/quiet InstallAllUsers=0 PrependPath=1 TargetDir={installationPath}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

        }
        else
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "sudo",
                Arguments = $"installer -pkg {installerName} -target /",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            // macOS: Open the installer package in the GUI
            var tempInstallationPath = Path.GetTempPath();
            var installerPath = Path.Combine(tempInstallationPath, installerName);

            process.StartInfo = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = installerPath, // Path to the installer package
                UseShellExecute = false
            };
            process.Start();
            process.WaitForExit(); // Optional, depends on whether you want to wait here or later
        }

        process.Start();
        process.WaitForExit();


    }

    private async Task InstallPythonLibrary(string libraryName)
    {
        var pythonPath = Preferences.Get("Python_Path", "/Library/Frameworks/Python.framework/Versions/3.10/bin/python3");

        if (string.IsNullOrEmpty(pythonPath))
        {
            await DisplayAlert("Error", "Failed to find Python installation path.", "OK");
            return;
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"-m pip install {libraryName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true, // Capture standard error as well
                UseShellExecute = false,
                CreateNoWindow = false,
            }
        };
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            // Handle the error
            // You can use the 'error' variable to get the error details
            await DisplayAlert("Installation Error", $"Failed to install {libraryName}. Error: {error}", "OK");
            Console.WriteLine("Installation Error", $"Failed to install {libraryName}. Error: {error}", "OK");
        }

    }

    private async Task UpgradePipAsync(string pythonPath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = "-m pip install --upgrade pip",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();

        // Optionally, read the output to capture any messages
        string output = await process.StandardOutput.ReadToEndAsync();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            // There was an error, handle it as needed
            await DisplayAlert("Error", "Failed to upgrade pip.", "OK");
        }
        else
        {
            // pip was upgraded successfully
            await DisplayAlert("Success", "pip was upgraded successfully.", "OK");
        }
    }


    private async Task<string> GetPythonInstallationPath()
    {
        string[] possibleExecutables = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                                       new[] { "python.exe", "python3.exe" } :
                                       new[] { "python", "python3" };

        foreach (var executable in possibleExecutables)
        {
            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: Custom installation path
                var customPythonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "aimee", "Python");
                var pythonExecutable = Path.Combine(customPythonPath, executable);

                if (File.Exists(pythonExecutable))
                {
                    return pythonExecutable; // Return if found in custom path
                }

                var process = new Process { StartInfo = processStartInfo };
                process.Start();
                string pythonPath = await process.StandardOutput.ReadLineAsync();
                process.WaitForExit();
            }
            else
            {
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = "-c \"which python3.10\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                        }
                    };
                    process.Start();
                    string pythonExecutable = await process.StandardOutput.ReadLineAsync();
                    process.WaitForExit();

                }
            }

        }

        // Python with specific version not found
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? null :
               "/Library/Frameworks/Python.framework/Versions/3.10/bin/python3";
    }

}