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
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui;


namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {

        string userPrompt;
        public static string apiKey { get; set; } = Preferences.Get("api_key", "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");
        public bool is_night_mode_on { get; set; } = Preferences.Get("night_mode", false);
        public bool is_code_visible { get; set; } = Preferences.Get("see_code", false);
        public string interpreter_model { get; set; } = Preferences.Get("interpreter_model", "gpt-3.5-turbo");

        //memory
        public long memory_count { get; set; } = Preferences.Get("memory_character_count", (long)0);


        AnimatedGif animatedGif;

        public bool is_executing_code = false;

        //interper
        Frame interpreterOutputFrame;
        private bool isFirstUpdate = true;
        // Image loadingGif;
        Label resultLabel;

        private GoogleTTSPlayer ttsPlayer = new GoogleTTSPlayer();  // Initializing TTS

        private GoogleSTTPlayer sttPlayer = new GoogleSTTPlayer(); // Initializing STT
        private AudioRecorder audioRecorder;
        public MainPage()
        {
            InitializeComponent();
            // InitializeAudioRecorder();
            Shell.SetNavBarIsVisible(this, false);
            Connectivity.ConnectivityChanged += OnConnectivityChanged;

        }



        //STT
        /*
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
        */

        //USER PROMPT INPUT
        private async void UserInputBox_Completed(System.Object sender, System.EventArgs e) 
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                userPrompt = UserInput.Text;
                UserInput.Text = "";

                if (!string.IsNullOrEmpty(userPrompt))
                {


                    Debug.WriteLine("memory_count" + memory_count);
                    
                    if (memory_count <= TrimMemoryCS.MaxCharacters)
                    {
                        AddChatBoxes();
                        await TrimMemoryCS.TrimMemoryFile();

                    }
                    else
                    {
                        await TrimMemoryCS.TrimMemoryFile();
                        AddChatBoxes();
                    }


                }
            }
            else
            {
               // NoInternetFrame.IsVisible = true;
                await Task.Delay(3000);
                //NoInternetFrame.IsVisible = false;
            }
        }

        private void AddChatBoxes()
        {
            var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");
            ScrollView chatScrollView = (ScrollView)FindByName("ChatScrollView");

            UserChatBoxUI.AddUserChatBoxToUI(gridLayout, chatScrollView, userPrompt);
            AddInterpreterChatBoxToUI();
        }





        //INTERPRETER CHAT UI
        private async void AddInterpreterChatBoxToUI()
        {

            var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");

            // Add a new RowDefinition for each chat bubble
            gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Get screen dimensions
            var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;

            // Calculate responsive margin
            double relativeMargin = screenWidth * 0.1; // 10% of screen width

            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            gradientBrush.GradientStops.Add(new GradientStop { Color = new Color(0.690f, 0.502f, 0.718f), Offset = 0 });
            gradientBrush.GradientStops.Add(new GradientStop { Color = new Color(0.937f, 0.804f, 0.882f), Offset = 1 });

            var customShadow = new Shadow
            {
                Radius = 10,
                Opacity = 0.6f,
                Brush = new SolidColorBrush(new Color(0.690f, 0.502f, 0.718f)),
                Offset = new Point(5, 5)
            };

            animatedGif = new AnimatedGif("MauiApp2.Resources.Images.genesis_loading.gif");
            animatedGif.WidthRequest = 80;
            animatedGif.HeightRequest = 80;

            resultLabel = new Label
            {
                Text = "",
                TextColor = Color.FromArgb("#fff"),
                FontFamily = "Montserrat-Light",
                IsVisible = false
            };

            interpreterOutputFrame = new Frame
            {
                //HorizontalOptions = LayoutOptions.FillAndExpand,  
                HasShadow = true,
                Shadow = customShadow,
                Background = gradientBrush,
                //Margin = new Thickness(0, 0, relativeMargin, 0),  // Use the responsive margin
                BorderColor = Color.FromRgba(255, 255, 255, 0),
                //VerticalOptions = LayoutOptions.FillAndExpand,  // Make it responsive
                Margin = new Thickness(0, 0, relativeMargin, 0),  // Add 20 units of space at the bottom

            };

            interpreterOutputFrame.Content = new Microsoft.Maui.Controls.StackLayout
            {
                Children = { animatedGif, resultLabel }
            };

            // Add the new Frame to the Grid

            gridLayout.Children.Add(interpreterOutputFrame);
            Microsoft.Maui.Controls.Grid.SetRow(interpreterOutputFrame, gridLayout.RowDefinitions.Count - 1);
            Microsoft.Maui.Controls.Grid.SetColumn(interpreterOutputFrame, 0);

            await RunPythonScriptAsync();

            


            this.Dispatcher.Dispatch(async () =>
            {
                Debug.WriteLine("End of Interpreter ChatBOX UI");

                interpreterOutputFrame.ForceLayout();
                ChatScrollView.ForceLayout();

                await Task.Delay(500);  // Optional: give it time to layout if needed

                await ChatScrollView.ScrollToAsync(0, gridLayout.Height, true);
            });
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

        private async Task<string> RunPythonScriptAsync()
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

            //THIS FUNCTION SHOULD CALL RUNPYTHONSCRIPT

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
            //Debug.WriteLine($"Concatenated Chunks: {IConcatenatedChunks}");  // Debug line
            var decodedJson = JsonDecoder.DecodeConcatenatedJSON(IConcatenatedChunks);  // DECODES ENTIRE INTERPRETER MESSAGE

            //PlayAudioFromText(decodedJson);

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
                try
                {
                    var validJson = MakeValidJson(jsonObject);
                    UpdateInterpreterUI(validJson);
                }
                catch (JsonReaderException ex)
                {
                    Debug.WriteLine($"JSON parsing error: {ex.Message}");
                    Debug.WriteLine($"Problematic JSON: {jsonObject}");
                }
            }
            //Debug.WriteLine($"Remaining Buffer Content: {jsonBuffer}");  // Debug line
        }

        private string MakeValidJson(string jsonObject)
        {
            try
            {
                // Replace 'True' with 'true' 
                jsonObject = jsonObject.Replace("'start_of_code': True", "'start_of_code': true");

                jsonObject = jsonObject.Replace("'end_of_execution': True", "'end_of_execution': true");

                jsonObject = jsonObject.Replace("'start_of_message': True", "'start_of_message': true");

                jsonObject = jsonObject.Replace("'end_of_message': True", "'end_of_message': true");


            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                Debug.WriteLine($"JSON parsing error: {ex.Message}");
                Debug.WriteLine($"Problematic JSON: {jsonObject}");
            }

            // Debug.WriteLine("MakeValidJson: " + jsonObject);

            return jsonObject;
        }


        private void UpdateInterpreterUI(string jsonObject) //this function is inside a loop, so we need to be careful to not load it with too much stuff (preferably almost nothing)
        {

            Debug.WriteLine(jsonObject);


            this.Dispatcher.Dispatch(() =>
            {

                if (isGIFEnabled)
                {
                    //loadingGif.IsVisible = false;
                    resultLabel.IsVisible = true;  // Show the label
                    animatedGif.IsVisible = false;
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

                    //end-start of message
                    var start_of_message = json["start_of_message"]?.ToObject<bool>();
                    var end_of_message = json["end_of_message"]?.ToObject<bool>();

                    //code
                    var end_of_execution = json["end_of_execution"]?.ToObject<bool>();
                    var start_of_code = json["start_of_code"]?.ToObject<bool>();


                    // start code
                    if (start_of_code == true)
                    {
                        Debug.WriteLine("START OF CODE is true");

                        is_executing_code = true;
                        resultLabel.Text = "coding...";

                        AddInterpreterCodeBoxToUI();
                    }


                    // end code
                    if (end_of_execution == true)
                    {

                        DeactivateInterpreterCodeBox();

                    }
                    //ends the UI


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

                        if (message.Contains("\n"))
                        {

                            Debug.WriteLine("MESSAGE CONTAINS /N");

                            //interpreterOutputFrame.ForceLayout();
                            //ChatScrollView.ForceLayout();


                            //var chatLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");
                            //ChatScrollView.ScrollToAsync(0, chatLayout.Height, true);

                            this.Dispatcher.Dispatch(async () =>
                            {
                                Debug.WriteLine("DISPATCH FORCE UI");

                                ChatScrollView.ForceLayout();
                                interpreterOutputFrame.ForceLayout();

                               // await Task.Delay(500);  // Optional: give it time to layout if needed
                                var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");
                                await ChatScrollView.ScrollToAsync(0, gridLayout.Height, true);
                            });

                        }
                    }

                    if (start_of_message == true)
                    {

                    }
                    
                    if (end_of_message == true)
                    {
                        // Assuming you want to reload the layout of ChatScrollView
                        //ChatScrollView.ForceLayout();

                    }
                    

                    if (output != null)
                    {
                        //write the output in the code box?
                    }


                }
                catch (JsonReaderException ex)
                {
                    Debug.WriteLine("Json parser exception" + ex.Message);
                    Debug.WriteLine("JSON object: " + jsonObject);

                }



            });
        }

        private Frame interpreterCodeFrame; // Declare a class-level variable to hold the frame

        private void AddInterpreterCodeBoxToUI()
        {
            Debug.WriteLine("AddInterpreterCodeBoxToUI has been called");

            var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");

            var newRow = new RowDefinition { Height = GridLength.Auto };
            gridLayout.RowDefinitions.Add(newRow);
            int currentRow = gridLayout.RowDefinitions.Count - 1;

            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };

            gradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb("#121B3F"), Offset = 1 });
            gradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb("#B180B8"), Offset = 0 });


            var customShadow = new Shadow
            {
                Radius = 10,
                Opacity = 0.6f,
                Brush = new SolidColorBrush(new Color(0.690f, 0.502f, 0.718f)),  // #EFCDE1
                Offset = new Point(5, 5)  // Offset of 5 pixels to the right and down
            };

            //Gif Animation
            var CodeGif = new Image
            {

                Source = new FileImageSource
                {
                    File = "genesis_code.gif"
                },
                WidthRequest = 80,
                HeightRequest = 80,
                IsAnimationPlaying = true,
                HorizontalOptions = LayoutOptions.Start

            };

            CodeGif.IsVisible = true;
            CodeGif.IsAnimationPlaying = true;

            var animatedGif2 = new AnimatedGif("MauiApp2.Resources.Images.genesis_loading.gif");
            animatedGif2.WidthRequest = 80;
            animatedGif2.HeightRequest = 80;

            
            interpreterCodeFrame = new Frame
            {
                HorizontalOptions = LayoutOptions.Start,
                HasShadow = true,
                Shadow = customShadow,
                Background = gradientBrush,
                Margin = new Thickness(0, 0, 80, 0),
                BorderColor = Color.FromRgba(255, 255, 255, 0),


            };

            interpreterCodeFrame.Content = new Microsoft.Maui.Controls.StackLayout
            { 
                Children = { animatedGif2 }
            };

            gridLayout.Children.Add(interpreterCodeFrame);
            Microsoft.Maui.Controls.Grid.SetRow(interpreterCodeFrame, currentRow);  // Use the currentRow variable
            Microsoft.Maui.Controls.Grid.SetColumn(interpreterCodeFrame, 0);


        }

        private void DeactivateInterpreterCodeBox()
        {
            var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");

            if (interpreterCodeFrame != null)
            {
                gridLayout.Children.Remove(interpreterCodeFrame);
                interpreterCodeFrame = null; // Release the reference
            }
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
                //NoInternetFrame.IsVisible = false;
            }
            else
            {
                // Connection to internet is not available
               // NoInternetFrame.IsVisible = true;
            }
        }
    }
}