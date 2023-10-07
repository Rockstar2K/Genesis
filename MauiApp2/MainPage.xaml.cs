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
        Frame outputFrame;

        private GoogleTTSPlayer ttsPlayer = new GoogleTTSPlayer();  // Initializing TTS
        public MainPage()
        {
            InitializeComponent();
        }


        private void InputBox_Completed(System.Object sender, System.EventArgs e) //when the input is sended
        {
            userPrompt = InputBox.Text; 
            InputBox.Text = ""; //it deletes the text of the entry once sended

            AddUserChatBoxToUI(userPrompt);
            //PlayUserPrompt(userPrompt);  // method to play the user prompt in TTS
            AddInterpreterChatBoxToUI(userPrompt);

        }

        private void AddUserChatBoxToUI(string input)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#F2CFE2"),
                BorderColor = Color.FromArgb("#F2CFE2"),
                Margin = new Thickness(80, 0, 0, 0), //left, top, right, bottom
                                                     // ... other styling ...
                Content = new Label
                {
                    Text = input,
                    TextColor = Color.FromArgb("#121B3F"),
                }
            };

            var stackLayout = (VerticalStackLayout)FindByName("ChatLayout");
            stackLayout.Children.Add(frame);

            frame.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#121B3F")),
                Offset = new Point(0, 5),
                Radius = 15,
                Opacity = 0.1f
            };

        }


        private async void AddInterpreterChatBoxToUI(string userPrompt)
        {
            var stackLayout = (VerticalStackLayout)FindByName("ChatLayout");

            outputFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#B280B9"),
                BorderColor = Color.FromArgb("#B280B9"),
                Margin = new Thickness(0, 0, 80, 0),
                Content = new Label
                {
                    Text = "Waiting for response...",
                    TextColor = Color.FromArgb("#fff"),
                }
            };

            stackLayout.Children.Add(outputFrame);

            var result = await RunPythonScriptAsync(userPrompt, apiKey);
            UpdateUI(result);  // this will replace "Waiting for response..." with the result
        }



        private async void PlayAudioPrompt(string text)
        {
            byte[] audioData = await ttsPlayer.GetAudioData(text);  // Assuming you have this method set up
            ttsPlayer.PlayAudio(audioData);
        }


        private async Task<string> RunPythonScriptAsync(string message, string apiKey)
        {
            string pythonPath;
            string scriptPath;
            string projectDirectory;

            if (OperatingSystem.IsMacCatalyst())
            {
                //paths for MacOS
                projectDirectory = "/Users/n/Desktop/AGI/MauiApp2/";
                scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");
                pythonPath = "/Users/n/anaconda3/bin/python";
            }
            else if (System.OperatingSystem.IsWindows())
            {
                //paths for Windows
                projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\"));
                scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");
                pythonPath = "C:\\Program Files\\Python311\\python.exe";
            }
            else
            {
                // Unsupported OS
                return string.Empty;
            }

            return await ExecuteScriptAsync(pythonPath, scriptPath, message, apiKey);
        }


        private async Task<string> ExecuteScriptAsync(string pythonPath, string scriptPath, string message, string apiKey)
        {
            var outputBuilder = new StringBuilder();

            await Task.Run(async () =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = $"{pythonPath}",
                        Arguments = $"\"{scriptPath}\" \"{message}\" \"{apiKey}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                process.Start();

                using (var reader = process.StandardOutput)
                {
                    char[] buffer = new char[256];  // Adjust buffer size as needed
                    int charsRead;
                    while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        var chunk = new string(buffer, 0, charsRead);
                        UpdateUI(chunk);
                        outputBuilder.Append(chunk);
                    }
                }


                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine("Error/Debug output: " + error);
                }
            });

            return outputBuilder.ToString();
        }

        private void UpdateUI(string text)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var label = outputFrame.Content as Label;
                if (label != null)
                {
                    label.Text += text;
                }
            });
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
