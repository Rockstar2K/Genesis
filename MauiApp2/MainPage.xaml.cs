using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SkiaSharp.Extended.UI.Controls;
using Microsoft.Maui.Graphics;


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
            gradientBrush.GradientStops.Add(new GradientStop { Color = new Color(0.690f, 0.502f, 0.718f), Offset = 0});
            gradientBrush.GradientStops.Add(new GradientStop { Color = new Color(0.937f, 0.804f, 0.882f), Offset = 1});

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
                WidthRequest = 100,
                HeightRequest = 100,
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
                Text = "",
                TextColor = Color.FromArgb("#fff"),
                FontFamily = "Montserrat-Light",
                IsVisible = false  // Hide the label initially
            };


            outputFrame = new Frame
            {
                HorizontalOptions = LayoutOptions.Start,
                HasShadow = true,
                Shadow = customShadow,
                Background = gradientBrush,
                Margin = new Thickness(0, 0, 80, 0),
                BorderColor = Color.FromRgba(255, 255, 255, 0),

            //  Content 

        };

            outputFrame.Content = new StackLayout
            {
                Children = { loadingGif, resultLabel } //USING GIF FOR NOW
            };

            stackLayout.Children.Add(outputFrame);
            var interpreterResponse = await RunPythonScriptAsync(userPrompt, apiKey);

        }

        private async void PlayAudioFromText(string text)
        {
            try
            {
                Debug.WriteLine("PlayAudioFromText is called, now it should play: " + text);
                await ttsPlayer.PlayAudioFromText(text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred: " + ex.Message);
            }
        }

        private async Task<string> RunPythonScriptAsync(string userPrompt, string apiKey)
        {
            Debug.WriteLine($"RunPythonScriptAsync called with message: {userPrompt}, apiKey: {apiKey}");  // Monitoring line

            string pythonPath;
            string scriptPath;
            string projectDirectory;

            if (OperatingSystem.IsMacCatalyst())
            {
                projectDirectory = "/Users/n/Desktop/AGI/MauiApp2/";
                scriptPath = Path.Combine(projectDirectory, "interpreter_wrapper.py");
                pythonPath = "/Users/n/anaconda3/bin/python";
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
                        var interpreterChunk = new string(buffer, 0, charsRead);
                        Debug.WriteLine($"The model returned: {interpreterChunk}");  // Monitoring line
                        UpdateUI(interpreterChunk);
                        outputBuilder.Append(interpreterChunk);
                    }

                }

                string interpreterConcatenatedChunks = outputBuilder.ToString();
                decodeConcatenatedJSON(userPrompt, interpreterConcatenatedChunks); //we decode the final json message to use it in SSDconversation and the TTS


                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine("Error/Debug output: " + error);
                }
            });

            return outputBuilder.ToString();
        }

        [Obsolete]
        private void UpdateUI(string intepreterChunk) //this function is inside a loop, so we need to be careful to not load it with too much stuff (preferably almost nothing)
        {

            Device.BeginInvokeOnMainThread(() =>
            {
                loadingGif.IsVisible = false;
                lottieView.IsVisible = false;  // Hide the loading image
                resultLabel.IsVisible = true;  // Show the label

                //Debug.WriteLine("Received text: " + text);

                if (string.IsNullOrEmpty(intepreterChunk))
                {
                    Debug.WriteLine("Text is null or empty updateUI");
                    return;
                }

                try
                {

                    var json = JObject.Parse(intepreterChunk);
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
            //PlayAudioFromText(fullMessage.ToString());
        }

       

        Queue<string> lastRecords = new Queue<string>();
        int maxRecords = 30; // Set the number of records here

        private void SSDconversation(string userPrompt, string interpreterResponse)
        {

            // Record
            string newRecord = $"User Prompt: {userPrompt}\nResponse: {interpreterResponse}\n";

            // Add to queue
            if (lastRecords.Count >= maxRecords) //change this for more/less records
            {
                lastRecords.Dequeue(); // Remove oldest if more than lastRecords
            }
            lastRecords.Enqueue(newRecord); // Add new record

            // Path
            string ssdDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../.."));
            string ssdFile = Path.Combine(ssdDirectory, "all_user_prompts_and_responses.txt");

            // Write last records to file
            File.WriteAllText(ssdFile, string.Join("", lastRecords));
            Debug.WriteLine(ssdFile);
            
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
