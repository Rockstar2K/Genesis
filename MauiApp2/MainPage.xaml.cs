using System;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SkiaSharp.Extended.UI.Controls;
using Microsoft.Maui.Graphics;
using System.Reflection.Metadata;
using Plugin.Maui.Audio;
using MauiApp2.SCRIPTS;
using MauiApp2.CustomControls;

namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {

        string userPrompt;
        public static string apiKey     { get; set; } = Preferences.Get("api_key", "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");
        public bool is_night_mode_on    { get; set; } = Preferences.Get("night_mode", false);
        public bool is_code_visible     { get; set; } = Preferences.Get("see_code", false);
        public string interpreter_model { get; set; } = Preferences.Get("interpreter_model", "gpt-3.5-turbo");

        public bool is_executing_code = false;

        Frame outputFrame;
        private bool isFirstUpdate = true;
        Image loadingGif;
        AnimatedGif animatedGif;
        Label resultLabel;
        SKLottieView lottieView;

        private GoogleTTSPlayer ttsPlayer = new GoogleTTSPlayer();  // Initializing TTS

        
        private GoogleSTTPlayer sttPlayer = new GoogleSTTPlayer(); // Initializing STT
        private AudioRecorder audioRecorder;
        public MainPage()
        {
            InitializeComponent();
            InitializeAudioRecorder();
            Shell.SetNavBarIsVisible(this, false);
            Connectivity.ConnectivityChanged += OnConnectivityChanged;

        }
        //STT
        private void InitializeAudioRecorder()
        {
            var audioManager = new AudioManager();
            audioRecorder = new AudioRecorder(audioManager);  // Initializing audio recorder
        }

        string recordedAudioFilePath = "";

        private async void OnStartRecordingClicked(object sender, EventArgs e)
        {

            if (OperatingSystem.IsWindows())
            {
                recordedAudioFilePath = "C:\\Users\\thega\\source\\repos\\MauiApp2\\MauiApp2\\Resources\\Audio\\recording.wav";
            }

            await audioRecorder.StartRecordingAsync(recordedAudioFilePath);

        }

        private async void OnStopRecordingClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("OnStopRecordingClicked, recordedAudioFilePath: " + recordedAudioFilePath);
            var audioSource = await audioRecorder.StopRecordingAsync();

            string convertedAudioFilePath = "C:\\Users\\thega\\source\\repos\\MauiApp2\\MauiApp2\\Resources\\Audio\\Converted\\recording.wav";

            await audioRecorder.ConvertAudio(recordedAudioFilePath, convertedAudioFilePath);

            var transcription = await sttPlayer.ConvertSpeechToTextAsync(recordedAudioFilePath);
            Debug.WriteLine("transcription: " + transcription);

        }
        //STT

        //USER PROMPT INPUT
        private async void InputBox_Completed(System.Object sender, System.EventArgs e) //when the input is sended
        {
            var current = Connectivity.NetworkAccess;
            // Connection to internet is available
            if (current == NetworkAccess.Internet)
            {
            userPrompt = InputBox.Text; 
            InputBox.Text = ""; //it deletes the text of the entry once sended

            var stackLayout = (VerticalStackLayout)FindByName("ChatLayout");
            UserChatBoxUI.AddUserChatBoxToUI(stackLayout, userPrompt); // ADD USER CHAT
            AddInterpreterChatBoxToUI(userPrompt);
            }
            else
            {
                // Connection to internet is not available
                NoInternetFrame.IsVisible = true;
                await Task.Delay(3000);  // Wait for 3 seconds
                NoInternetFrame.IsVisible = false;
            }
        }


       

        //INTERPRETER CHAT UI
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
                WidthRequest = 200,
                HeightRequest = 200,
                RepeatCount = -1,  // Set to -1 to repeat the animation indefinitely
                RepeatMode = SKLottieRepeatMode.Restart  // Restart the animation after it completes
            };

            lottieView.IsAnimationEnabled = true;
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string resourcePath = Path.Combine(appDirectory, "Contents", "Resources", "genesis_loading.gif");
            animatedGif = new AnimatedGif("MauiApp2.Resources.Images.genesis_loading.gif");
            animatedGif.WidthRequest = 100;
            animatedGif.HeightRequest = 100;

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
                Children = { animatedGif , resultLabel } //USING GIF FOR NOW
            };

            stackLayout.Children.Add(outputFrame);

            if (is_executing_code)
            {
                while (true)
                {
                    if (is_executing_code == false)
                    {
                        outputFrame.IsVisible = false;
                    }
                    Thread.Sleep(500);
                }    
            }
            else
            {
                await RunPythonScriptAsync(userPrompt, apiKey);
            }

        }

        //TTS
        public async void PlayAudioFromText(string text)
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

        //INITIALIZES PYTHON

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

            return await ExecuteScriptAsync(pythonPath, scriptPath, userPrompt, apiKey, interpreter_model);
        }

        //EXECUTES PYTHON

        bool isGIFEnabled = false; // Inicializa la variable 

        private async Task<string> ExecuteScriptAsync(string pythonPath, string scriptPath, string userPrompt, string apiKey, string interpreter_model)
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
                        Arguments = $"\"{scriptPath}\" \"{userPrompt}\" \"{apiKey}\" \"{interpreter_model}\"",
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
                        //Debug.WriteLine($"The model returned: {interpreterChunk}");  // Monitoring line
                        ProcessChunk(interpreterChunk);
                        outputBuilder.Append(interpreterChunk);

                        isGIFEnabled = true; // Establece la bandera para que no se llame nuevamente
                        
                    }

                    isGIFEnabled = false;
                }

                
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine("Error/Debug output: " + error);
                }
            });

            string IConcatenatedChunks = outputBuilder.ToString();
            Debug.WriteLine($"Concatenated Chunks: {IConcatenatedChunks}");  // Debug line
            var decodedJson = JsonDecoder.DecodeConcatenatedJSON(IConcatenatedChunks);  // DECODES ENTIRE INTERPRETER MESSAGE

            PlayAudioFromText(decodedJson);

            return outputBuilder.ToString();
        }

        //UPDATES INTERPRETER UI

        private StringBuilder jsonBuffer = new StringBuilder();

        private void ProcessChunk(string chunk)
        {
            jsonBuffer.Append(chunk);
            ExtractAndProcessJsonObjects();
        }

        private void ExtractAndProcessJsonObjects()
        {
            string bufferContent = jsonBuffer.ToString();
            int lastObjectEndIndex = bufferContent.LastIndexOf('}');
            if (lastObjectEndIndex < 0) return;  // No complete object found

            string objectsString = bufferContent.Substring(0, lastObjectEndIndex + 1);
            jsonBuffer = new StringBuilder(bufferContent.Substring(lastObjectEndIndex + 1));  // Keep remaining content

            // Split based on '}' to get individual objects, then add '}' back to each object
            string[] jsonObjects = objectsString.Split(new[] { '}' }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(objStr => objStr + "}").ToArray();
            foreach (string jsonObject in jsonObjects)
            {
                UpdateUI(jsonObject);
            }
            Debug.WriteLine($"Remaining Buffer Content: {jsonBuffer.ToString()}");  // Debug line
        }



        private void UpdateUI(string jsonObject) //this function is inside a loop, so we need to be careful to not load it with too much stuff (preferably almost nothing)
        {

            this.Dispatcher.Dispatch(() =>
            {
                
                if (isGIFEnabled)
                {
                    loadingGif.IsVisible    = false;
                    animatedGif.IsVisible   = false;
                    lottieView.IsVisible    = false;  // Hide the loading image
                    resultLabel.IsVisible   = true;  // Show the label
                }
                
                
                if (string.IsNullOrEmpty(jsonObject))
                {
                    Debug.WriteLine("Text is null or empty updateUI");
                    return;
                }

                try
                {
                    var json = JObject.Parse(jsonObject);

                    var message = json["message"]?.ToString();
                    var language = json["language"]?.ToString();
                    var code = json["code"]?.ToString();
                    var executing = json["executing"];
                    var active_line = json["active_line"];
                    var output = json["output"]?.ToString();
                    var end_of_execution = json["end_of_execution"]?.ToString();
                    var start_of_message = json["start_of_message"];
                    var start_of_code = json["start_of_code"];
                
                    //Debug.WriteLine($"Updating UI with: {message}");  // Monitoring line
                    if (start_of_message != null)
                    {
                    }
                    else if (start_of_code != null)
                    {
                    }
                    else if (message != null)
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
                    else if (language != null)
                    { }
                    else if (code != null)
                    { }
                    else if (executing != null)
                    {
                        Console.WriteLine("Code is being executed");
                        is_executing_code = true;
                        //AddInterpreterChatBoxToUI("");
                        //Create a new message frame to show the loading animation and then the final message

                    }
                    else if (active_line != null)
                    { }
                    else if (output != null)
                    { }
                    else if (end_of_execution != null)
                    {
                        is_executing_code = false;
                    }
                    else
                    {
                    }
                }
                catch (JsonReaderException ex)
                {
                    Debug.WriteLine("Json parser exception" + ex.Message);
                    Debug.WriteLine("JSON object: " + jsonObject);

                }

            });
        }



        private async void Settings_Pressed(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("Settings");
                    var currentLocation = Shell.Current.CurrentState.Location;
                    Console.WriteLine(currentLocation);
                }
                else
                {
                    Console.WriteLine("Shell.Current is null.");
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Null reference exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred: " + ex.Message);
            }
        }


        void Voice_Option_Pressed(System.Object sender, System.EventArgs e)
        {
        }

        void Chat_Option_Pressed(System.Object sender, System.EventArgs e)
        {
        }

        void Welcome_Button_Clicked(System.Object sender, System.EventArgs e)
        {
            App.Current.MainPage = new NavigationPage(new Welcome_Page());

        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                // Connection to internet is available
                NoInternetFrame.IsVisible = false;
            }
            else
            {
                // Connection to internet is not available
                NoInternetFrame.IsVisible = true;
            }
        }
    }
}
