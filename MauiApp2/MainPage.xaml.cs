using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Text;
using System.IO;


namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {

        string userPrompt;
        readonly string apiKey = "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";
        private GoogleTTSPlayer ttsPlayer = new GoogleTTSPlayer();  // Initializing TTS
        public MainPage()
        {
            InitializeComponent();
        }

        /*
        private void OnPromptBtnClicked(object sender, EventArgs e)
        {
            userPrompt = InputBox.Text;
            Output.Text = "Waiting for response...";  // Set the text to "Waiting for response"
            TestPythonCode(userPrompt);  // Pass userPrompt to TestPythonCode
        }
        */

        private void InputBox_Completed(System.Object sender, System.EventArgs e)
        {
            userPrompt = InputBox.Text;
            Output.Text = "Waiting for response...";  // Set the text to "Waiting for response"
            TestPythonCode(userPrompt);  // Pass userPrompt to TestPythonCode
            //PlayUserPrompt(userPrompt);  // method to play the user prompt in TTS
        }

        private async void PlayUserPrompt(string text)
        {
            byte[] audioData = await ttsPlayer.GetAudioData(text);  // Assuming you have this method set up
            ttsPlayer.PlayAudio(audioData);
        }

        private async void TestPythonCode(string userPrompt)
        {
            InputBox.Text = "";
            var result = await RunPythonScriptAsync(userPrompt, apiKey);
            Define_Output(result);

        }

        private void Define_Output(string output)
        {
            Output.Text = output;
        }

        private async Task<string> RunPythonScriptAsync(string message, string apiKey)
        {

            if (OperatingSystem.IsMacCatalyst())
            {
                //paths
                string projectDirectory = "/Users/n/Desktop/Genesis Roco/Genesis/MauiApp2/";
                string scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");
                string pythonPath = "/Users/n/anaconda3/bin/python";

                return await Task.Run(() =>
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = $"{pythonPath}",
                            Arguments = $"\"{scriptPath}\" \"{message}\" \"{apiKey}\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,  // Re-enable error redirection
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            // weird characters are removed
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
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

            else if (System.OperatingSystem.IsWindows())
            {
                //paths
                string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\"));
                string scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");
                string pythonPath = "C:\\Program Files\\Python311\\python.exe";

                return await Task.Run(() =>
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = $"{pythonPath}",
                            Arguments = $"\"{scriptPath}\" \"{message}\" \"{apiKey}\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,  // Re-enable error redirection
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            // weird characters are removed
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
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
            else
            {
                return string.Empty;
            }

        }

        private void RAMconversation(string message, string result) //low memory for resend it with the prompt
        {
            if (System.OperatingSystem.IsWindows())
            {
                //path
                string ramDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\"));
                string ramFile = Path.Combine(ramDirectory, "user_prompts_and_responses.txt");

                string newContent = $"\nUser Prompt: {message}\nResponse: {result}\n";
                File.WriteAllText(ramFile, newContent);  // This will overwrite the existing content with the new content
                Debug.WriteLine(ramFile);
            }


        }


        private void SSDconversation(string message, string result) //stores all the conversation data
        {

            if (System.OperatingSystem.IsWindows())
            {
                //path
                string ssdDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\"));
                string ssdFile = Path.Combine(ssdDirectory, "all_user_prompts_and_responses.txt");

                // Append the new User Prompt and Response to the file
                File.AppendAllText(ssdFile, $"User Prompt: {message}\nResponse: {result}\n");
                Debug.WriteLine(ssdFile);

            }
        }

        void Settings_Pressed(System.Object sender, System.EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new Settings());
        }

        void Voice_Option_Pressed(System.Object sender, System.EventArgs e)
        {
        }

        void Chat_Option_Pressed(System.Object sender, System.EventArgs e)
        {
        }
    }
}
