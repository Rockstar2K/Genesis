
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
            var result = await RunPythonScriptAsync(userPrompt, apiKey);  
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
                        CreateNoWindow = true, //opens (or not) a cmd window
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
