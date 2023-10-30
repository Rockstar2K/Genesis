using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MauiApp2;

public partial class Welcome_Page : ContentPage
{
    private double progressValue;
    private const string PythonInstallerUrlWindows = "https://www.python.org/ftp/python/3.9.7/python-3.9.7-amd64.exe";
    private const string PythonInstallerUrlMac = "https://www.python.org/ftp/python/3.9.7/python-3.9.7-macosx10.9.pkg";
    private const string PythonInstallerNameWindows = "python-3.9.7-amd64.exe";
    private const string PythonInstallerNameMac = "python-3.9.7-macosx10.9.pkg";

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
            await DisplayAlert("Installation Complete", "Python has been installed successfully.", "OK");
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
                var progress = (int)((totalRead * 100) / totalLength);
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

}
