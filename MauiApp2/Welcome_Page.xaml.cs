using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MauiApp2;

public partial class Welcome_Page : ContentPage
{
    private double progressValue;
    private const string PythonInstallerUrlWindows = "https://www.python.org/ftp/python/3.12.0/python-3.12.0-amd64.exe";
    private const string PythonInstallerUrlMac = "https://www.python.org/ftp/python/3.12.0/python-3.12.0-macos11.pkg";

    private const string PythonInstallerNameWindows = "python-3.12.0-amd64.exe";
    private const string PythonInstallerNameMac = "python-3.12.0-macosx11.pkg";

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

            await DisplayAlert("Download Complete", "Python installer downloaded successfully.", "OK");
            InstallPython(installerName);
            string pythonPath = GetPythonInstallationPath();
            await DisplayAlert("Python Path", $"Python was installed at: {pythonPath}", "OK");

            //Update pip
            await UpgradePipAsync(pythonPath);

            // After Python installation is complete, install open-interpreter
            await InstallPythonLibrary("open-interpreter");
            progressBar.Progress = 1;
            await DisplayAlert("Installation Complete", "Open Interpreterhas been installed successfully.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
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
                var progress = (int)((totalRead * 90) / totalLength);
                progressBar.Progress = progress / 100.0;
            }
        }
    }

    private void InstallPython(string installerName)
    {
        var process = new Process();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = installerName,
                Arguments = "/quiet InstallAllUsers=1 PrependPath=1",
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
        }
        process.Start();
        process.WaitForExit();


    }

    private async Task InstallPythonLibrary(string libraryName)
    {
        string pythonPath = GetPythonInstallationPath();
        Preferences.Set("Python_Path", pythonPath); 
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
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        await process.WaitForExitAsync();
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


    private string GetPythonInstallationPath()
    {
        string[] possibleExecutables = { "python", "python3" };
        foreach (var executable in possibleExecutables)
        {
            var process = new Process();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/C where {executable}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };
            }
            else  // macOS and Linux
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = executable,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };
            }
            process.Start();
            string pythonPath = process.StandardOutput.ReadLine();
            process.WaitForExit();
            if (!string.IsNullOrEmpty(pythonPath))
            {
                return pythonPath;
            }
        }
        return null;
    }


}
