using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;


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
            userPrompt = InputBox.Text;
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
            string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\"));
            string scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");

            return await Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Program Files\Python311\python.exe",
                        Arguments = $"\"{scriptPath}\" \"{message}\" \"{apiKey}\"",
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

                RAMconversation(message, result);
                SSDconversation(message, result);


                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine("Error/Debug output: " + error);
                }

                return result + "\n" + error;  // Combine standard and error output
            });
        }

        private void RAMconversation(string message, string result) //low memory for resend it with the prompt
        {
            string filePath = @"C:\Users\thega\source\repos\MauiApp2\MauiApp2\user_prompts_and_responses.txt";
            string newContent = $"\nUser Prompt: {message}\nResponse: {result}\n";
            File.WriteAllText(filePath, newContent);  // This will overwrite the existing content with the new content
            Debug.WriteLine(filePath);
        }


        private void SSDconversation(string message, string result) //stores all the conversaton data
        {

            string filePath = @"C:\Users\thega\source\repos\MauiApp2\MauiApp2\all_user_prompts_and_responses.txt";
            // Append the new User Prompt and Response to the file
            File.AppendAllText(filePath, $"User Prompt: {message}\nResponse: {result}\n");
            Debug.WriteLine(filePath);

        }



    }
}
