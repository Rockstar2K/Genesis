using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SkiaSharp.Extended.UI.Controls;

namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {

        string userPrompt;
        public static string apiKey { get; set; } = "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";

        Frame outputFrame;
        private bool isFirstUpdate = true;
        Image loadingGif;
        Label resultLabel;
        SKLottieView lottieView;

        //private GoogleTTSPlayer ttsPlayer = new GoogleTTSPlayer();  // Initializing TTS
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

        private void AddUserChatBoxToUI(string userPrompt)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#F2CFE2"),
                BorderColor = Color.FromArgb("#F2CFE2"),
                Margin = new Thickness(80, 0, 0, 0), //left, top, right, bottom
                                                     // ... other styling ...
                Content = new Label
                {
                    Text = userPrompt,
                    FontFamily = "Montserrat-Light",
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
                Opacity = 0.6f
            };

        }


        private async void AddInterpreterChatBoxToUI(string userPrompt)
        {
            Debug.WriteLine($"AddInterpreterChatBoxToUI called with userPrompt: {userPrompt}");  // Monitoring line

            var stackLayout = (VerticalStackLayout)FindByName("ChatLayout");
            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            gradientBrush.GradientStops.Add(new GradientStop { Color = new Color(0.690f, 0.502f, 0.718f), Offset = 0.0f });  // #B080B7
            gradientBrush.GradientStops.Add(new GradientStop { Color = new Color(0.690f, 0.502f, 0.718f), Offset = 0.25f });  // #EFCDE1

            var customShadow = new Shadow
            {
                Radius = 10,
                Opacity = 0.6f,
                Brush = new SolidColorBrush(new Color(0.690f, 0.502f, 0.718f)),  // #EFCDE1
                Offset = new Point(5, 5)  // Offset of 5 pixels to the right and down
            };
            //Gif Animation
            loadingGif = new Image
            {

                Source = new FileImageSource
                {
                    File = "genesis_loading.gif"
                },
                WidthRequest = 200,
                HeightRequest = 200,
                IsAnimationPlaying = true,
                HorizontalOptions = LayoutOptions.Start
                
            };

            loadingGif.IsVisible = true;
            loadingGif.IsAnimationPlaying = true;

            //lottie Animation
            var source = new SKFileLottieImageSource
            {
                File = "https://lottie.host/440bfe79-8145-4324-9976-29d4d0830194/p57JQzlc3e.json"  // specify the path to your Lottie animation file
            };

            lottieView = new SKLottieView
            {
                Source = source,
                WidthRequest = 300,
                HeightRequest = 300,
                RepeatCount = -1,  // Set to -1 to repeat the animation indefinitely
                RepeatMode = SKLottieRepeatMode.Restart  // Restart the animation after it completes
            };

            lottieView.IsAnimationEnabled = true;

            resultLabel = new Label
            {
                Text = "Waiting for response...",
                TextColor = Color.FromArgb("#fff"),
                FontFamily = "Montserrat-Light",
                IsVisible = false,  // Hide the label initially
                LineBreakMode = LineBreakMode.WordWrap  // Add this line to wrap text

            };


            outputFrame = new Frame
            {
                HasShadow = true,
                Shadow = customShadow,
                Background = gradientBrush,
                BorderColor = Color.FromArgb("#B280B9"),
                Margin = new Thickness(0, 0, 80, 0),
                //  Content 
 
            };

            outputFrame.Content = new StackLayout
            {
                Children = { loadingGif, resultLabel } //USING GIF FOR NOW
            };

            stackLayout.Children.Add(outputFrame);



            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },  // For the Label
                    new ColumnDefinition { Width = GridLength.Auto }  // For the GIF
                }
            };




            var result = await RunPythonScriptAsync(userPrompt, apiKey);
        }


        /*
        private async void PlayAudioPrompt(string text)
        {
            byte[] audioData = await ttsPlayer.GetAudioData(text);  // Assuming you have this method set up
            ttsPlayer.PlayAudio(audioData);
        }
        */

        private async Task<string> RunPythonScriptAsync(string userPrompt, string apiKey)
        {
            Debug.WriteLine($"RunPythonScriptAsync called with message: {userPrompt}, apiKey: {apiKey}");  // Monitoring line

            string pythonPath;
            string scriptPath;
            string projectDirectory;

            if (OperatingSystem.IsMacCatalyst())
            {
                //Paths for MacOs
                projectDirectory = Environment.CurrentDirectory;
                scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");
                pythonPath = "/usr/local/bin/python";  // Assumes python is installed in a standard location
            }

            else if (System.OperatingSystem.IsWindows())
            {
                //paths for Windows
                projectDirectory = "C:\\Users\\thega\\source\\repos\\MauiApp2\\MauiApp2"; // Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\"));
                scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");
                pythonPath = "C:\\Program Files\\Python311\\python.exe";
            }
            else
            {
                // Unsupported OS
                return string.Empty;
            }

            return await ExecuteScriptAsync(pythonPath, scriptPath, userPrompt, apiKey);
        }


        private async Task<string> ExecuteScriptAsync(string pythonPath, string scriptPath, string userPrompt, string apiKey)
        {
            Debug.WriteLine($"ExecuteScriptAsync called with pythonPath: {pythonPath}, scriptPath: {scriptPath}, userPrompt: {userPrompt}, apiKey: {apiKey}");  // Monitoring line

            var outputBuilder = new StringBuilder();

            await Task.Run(async () =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = $"{pythonPath}",
                        Arguments = $"\"{scriptPath}\" \"{userPrompt}\" \"{apiKey}\"",
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
                        Debug.WriteLine($"The model returned: {chunk}");  // Monitoring line
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

            string concatenatedChunks = outputBuilder.ToString();
            decodeConcatenatedJSON(userPrompt, concatenatedChunks); //we decode the final json message to store it in SSDconversation

            return outputBuilder.ToString();
        }

        [Obsolete]
        private void UpdateUI(string chunks) //this function is inside a loop, so we need to be careful to not load it with too much stuff (preferably almost nothing)
        {

            Device.BeginInvokeOnMainThread(() =>
            {
                loadingGif.IsVisible = false;
                lottieView.IsVisible = false;  // Hide the loading image
                resultLabel.IsVisible = true;  // Show the label

                //Debug.WriteLine("Received text: " + text);

                if (string.IsNullOrEmpty(chunks))
                {
                    Debug.WriteLine("Text is null or empty updateUI");
                    return;
                }

                try
                {

                    var json = JObject.Parse(chunks);

                    //Debug.WriteLine("json: " + json);

                    var message = json["message"]?.ToString();
                    //Debug.WriteLine($"Updating UI with: {message}");  // Monitoring line

                        if (message != null)
                        {
                            if (isFirstUpdate)
                            {
                                resultLabel.Text = message;  // Set the text to the first message received
                                isFirstUpdate = false;
                            }
                            else
                            {
                                resultLabel.Text += message;  // Append subsequent messages
                            }
                        }
                    }
                catch (JsonReaderException ex)
                {
                    Debug.WriteLine("Json parser exception" + ex.Message); //JSON ERRORS HERE
                }
                
            });
        }

        
        private void decodeConcatenatedJSON(string userPrompt, string concatenatedChunks)
        {
            Debug.WriteLine("decodeJSON initialized with concatenatedChunks: " + concatenatedChunks);

            var fullMessage = new StringBuilder();  // Use StringBuilder for efficient string concatenation

            try
            {
                // Split concatenatedChunks into individual JSON strings
                var chunks = concatenatedChunks.Split(new string[] { "}" }, StringSplitOptions.None);

                foreach (var chunk in chunks)
                {
                    if (string.IsNullOrEmpty(chunk))
                        continue;

                    var validJson = chunk + "}";  // Add closing brace to make it valid JSON
                    var json = JObject.Parse(validJson);
                    var imessage = json["message"]?.ToString();

                    if (imessage != null)
                    {
                        fullMessage.Append(imessage);  // Append message to fullMessage
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                Debug.WriteLine("Json parser exception" + ex.Message);
            }

            Debug.WriteLine("decodeJSON: " + fullMessage.ToString());



            SSDconversation(userPrompt, fullMessage.ToString());
        }


        private void SSDconversation(string userPrompt, string interpreterResponse) //stores all the conversation data
        {

            if (System.OperatingSystem.IsWindows())
            {
                //path
                string ssdDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\"));
                string ssdFile = Path.Combine(ssdDirectory, "all_user_prompts_and_responses.txt");

                // Append the new User Prompt and Response to the file
                File.AppendAllText(ssdFile, $"User Prompt: {userPrompt}\nResponse: {interpreterResponse}\n");
                Debug.WriteLine(ssdFile);

            }
        }

        /*
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
        */


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
