/*
using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnPromptBtnClicked(object sender, EventArgs e)
        {
            TestPythonCode();
        }

        private void InputBox_Completed(System.Object sender, System.EventArgs e)
        {
            TestPythonCode();
        }

        private void TestPythonCode()
        {
            var result = RunPythonScript("What operating system are we on?");
            Define_Output(result);
        }

        private void Define_Output(string output)
        {
            Output.Text = output;
        }

        private string RunPythonScript(string message)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\Python311\python.exe",
                    Arguments = $"\"C:\\Users\\thega\\source\\repos\\MauiApp2\\MauiApp2\\interpreter_wrapper.py\" \"{message}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    //CreateNoWindow = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}
*/
using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {

        string userPrompt;
        string apiKey = "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";

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnPromptBtnClicked(object sender, EventArgs e)
        {
            var userPrompt = InputBox.Text;
            TestPythonCode(userPrompt);  // Pass userPrompt to TestPythonCode
        }

        private void InputBox_Completed(System.Object sender, System.EventArgs e)
        {
            var userPrompt = InputBox.Text;
            TestPythonCode(userPrompt);  // Pass userPrompt to TestPythonCode
        }

        private async void TestPythonCode(string userPrompt)
        {
            var result = await RunPythonScriptAsync(userPrompt, apiKey);  // Replace "your-api-key-here" with your actual API key
            Define_Output(result);
        }

        private void Define_Output(string output)
        {
            Output.Text = output;
        }

        private async Task<string> RunPythonScriptAsync(string message, string apiKey)
        {
            return await Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Program Files\Python311\python.exe",
                        Arguments = $"\"C:\\Users\\thega\\source\\repos\\MauiApp2\\MauiApp2\\interpreter_wrapper.py\" \"{message}\" \"{apiKey}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        //CreateNoWindow = true,
                    }
                };

                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return result;
            });
        }

    }
}
