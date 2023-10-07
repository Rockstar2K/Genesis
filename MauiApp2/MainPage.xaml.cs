using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.IO.Pipes;



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
                
            /*
                var a = new SKFileLottieImageSource();
                a.File = "dotnetbot.json";

                var lottieView = new SkiaSharp.Extended.UI.Controls.SKLottieView
                {
                    Source = a,
                    HeightRequest = 250,
                    RepeatCount = 99
                };
            */


            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#B280B9"),
                BorderColor = Color.FromArgb("#B280B9"),
                Margin = new Thickness(0, 0, 80, 0), //left, top, right, bottom
                //Content = lottieView

                
                Content = new Label
                {
                    Text = "Waiting for response...",
                    TextColor = Color.FromArgb("#fff"),
                }
                

            };

            stackLayout.Children.Add(frame);

            frame.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#121B3F")),
                Offset = new Point(0, 5),
                Radius = 15,
                Opacity = 0.1f
            };

            var result = await RunPythonScriptAsync(userPrompt, apiKey);
            ((Label)frame.Content).Text = result;
        }



        private async void PlayAudioPrompt(string text)
        {
            byte[] audioData = await ttsPlayer.GetAudioData(text);  // Assuming you have this method set up
            ttsPlayer.PlayAudio(audioData);
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
            /*
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
            */

            else if (System.OperatingSystem.IsWindows())
            {
                //paths
                string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\"));
                string scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");
                string pythonPath = "C:\\Program Files\\Python311\\python.exe";

                var pipeServer = new PipeServer();
                pipeServer.Start();

                return await Task.Run(async () =>
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

                    string error = process.StandardError.ReadToEnd();  // Re-enable error capture
                    process.WaitForExit();

                    // Wait for a message from the Python script
                    string result = await pipeServer.WaitForMessageAsync();

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

        static void Main(string[] args)
        {
            using (var server = new NamedPipeServerStream("MyPipe"))
            {
                Debug.WriteLine("Waiting for connection...");
                server.WaitForConnection();

                Debug.WriteLine("Connected!");

                using (var reader = new StreamReader(server))
                using (var writer = new StreamWriter(server))
                {
                    writer.AutoFlush = true;

                    while (true)
                    {
                        var messageFromClient = reader.ReadLine();
                        if (messageFromClient == null) break;  // Client has disconnected

                        Debug.WriteLine("Received: " + messageFromClient);

                        // Send a response
                        Debug.WriteLine("Response from server");
                    }
                }
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
