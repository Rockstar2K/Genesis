
using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Text;

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
            Output.Text = "Waiting for response...";  // Set the text to "Waiting for response"
            TestPythonCode(userPrompt);  // Pass userPrompt to TestPythonCode
        }

        private void InputBox_Completed(System.Object sender, System.EventArgs e)
        {
            var userPrompt = InputBox.Text;
            Output.Text = "Waiting for response...";  // Set the text to "Waiting for response"
            TestPythonCode(userPrompt);  // Pass userPrompt to TestPythonCode
        }

        private async void TestPythonCode(string userPrompt)
        {
            var result = await RunPythonScriptAsync(userPrompt, apiKey);  
            Define_Output(result);
            InputBox.Text = "";  // Clear the text in the Entry after the response has been received and displayed

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
                        RedirectStandardError = true,  // Re-enable error redirection
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();  // Re-enable error capture
                process.WaitForExit();

                // Guardar el mensaje del usuario y la respuesta en un archivo .txt
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "user_prompts_and_responses.txt", $"User Prompt: {message}\nResponse: {result}\n");
                Debug.WriteLine(AppDomain.CurrentDomain.BaseDirectory + "user_prompts_and_responses.txt"); //escribe la ruta de acceso al txt (para cachar donde está)


                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine("Error/Debug output: " + error);
                }

                return result + "\n" + error;  // Combine standard and error output
            });
        }


    }
}
