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
using Microsoft.Maui;
using CommunityToolkit.Maui;


namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {

        static string userPrompt;
        public static string apiKey { get; set; } = Preferences.Get("api_key", "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");
        public bool is_night_mode_on { get; set; } = Preferences.Get("night_mode", false);
        public bool is_code_visible { get; set; } = Preferences.Get("see_code", false);
        public static string interpreter_model { get; set; } = Preferences.Get("interpreter_model", "gpt-3.5-turbo");

        //memory
        public long memory_count { get; set; } = Preferences.Get("memory_character_count", (long)0);


        AnimatedGif animatedGif;

        public bool is_executing_code = false;

        //interpreter Chatbox
        Frame interpreterOutputFrame;
        private bool isFirstUpdate = true;
        Label resultLabel;


        //CODE
        bool isCodeFirstUpdate = true;
        Frame interpreterCodeFrame;
        Label codeLabel;



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


                    Debug.WriteLine("User Input memory count: " + memory_count);
                    
                    if (memory_count <= TrimMemoryCS.MaxCharacters) //si es menor se ejecuta addChatBoxes primero
                    {

                        Debug.WriteLine("Memory count LESS than MaxCharachters");

                        AddChatBoxes();
                        //await Task.Delay(10000);
                        //await TrimMemoryCS.TrimMemoryFile();

                    }
                    else
                    {

                        Debug.WriteLine("Memory count MORE than MaxCharachters");

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
            Debug.WriteLine("AddChatBoxes");

            var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");
            ScrollView chatScrollView = (ScrollView)FindByName("ChatScrollView");
            UserChatBoxUI.AddUserChatBoxToUI(gridLayout, chatScrollView, userPrompt);

             AddInterpreterChatBoxToUI(); //await for this and then call Execute?
        }


        //OPEN FILE
        private async void OpenFileButton_Clicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync();

            if (result != null)
            {
                var addfilePath = result.FullPath;
                Debug.WriteLine("add File Path: " + addfilePath);
                // Store filePath in a variable or use as needed
            }
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
            double relativeMargin = screenWidth * 0.05; // 10% of screen width

            var gradientBrush = new Microsoft.Maui.Controls.LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            gradientBrush.GradientStops.Add(new Microsoft.Maui.Controls.GradientStop { Color = Color.FromArgb("#F0CEE2"), Offset = 1 });
            gradientBrush.GradientStops.Add(new Microsoft.Maui.Controls.GradientStop { Color = Color.FromArgb("#B180B8"), Offset = 0 });

            var customShadow = new Microsoft.Maui.Controls.Shadow
            {
                Radius = 10,
                Opacity = 0.6f,
                Brush = new Microsoft.Maui.Controls.SolidColorBrush(new Color(0.690f, 0.502f, 0.718f)),
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
                Margin = new Thickness(20, 0, relativeMargin, 0),  // left, top, right, bottom

            };

            interpreterOutputFrame.Content = new Microsoft.Maui.Controls.StackLayout
            {
                Children = { animatedGif, resultLabel }
            };

            // Add the new Frame to the Grid

            gridLayout.Children.Add(interpreterOutputFrame);
            Microsoft.Maui.Controls.Grid.SetRow(interpreterOutputFrame, gridLayout.RowDefinitions.Count - 1);
            Microsoft.Maui.Controls.Grid.SetColumn(interpreterOutputFrame, 0);

            await ExecuteScriptAsync();


            Debug.WriteLine("End of Interpreter ChatBOX UI");

            //interpreterOutputFrame.ForceLayout();
            //ChatScrollView.ForceLayout();

            await Task.Delay(500);  // Optional: give it time to layout if needed

            await ChatScrollView.ScrollToAsync(0, gridLayout.Height, true);
            
        }

        private Task AddInterpreterCodeBoxToInterpreterOutputFrame()
        {
            Debug.WriteLine("AddInterpreterCodeBoxToInterpreterOutputFrame");

            codeLabel = new Label
            {
                Text = "",
                TextColor = Color.FromArgb("#fff"),
                FontSize = 12,
                FontFamily = "Montserrat-Light", //CONSOLAS TIPOGRAPHY
                IsVisible = false
            };

            codeLabel.IsVisible = true;

            var gradientBrush = new Microsoft.Maui.Controls.LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };

            gradientBrush.GradientStops.Add(new Microsoft.Maui.Controls.GradientStop { Color = Color.FromArgb("#5AFFFFFF"), Offset = 1 });
            gradientBrush.GradientStops.Add(new Microsoft.Maui.Controls.GradientStop { Color = Color.FromArgb("#1AFFFFFF"), Offset = 0 });

            interpreterCodeFrame = new Frame
            {
                Background = gradientBrush,
                BorderColor = Color.FromRgba(255, 255, 255, 0),
                Margin = new Thickness(0, 10, 0, 10),  // // left, top, right, bottom
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,


            };

            interpreterCodeFrame.Content = new Microsoft.Maui.Controls.StackLayout
            {
                Children = { codeLabel }
            };

            var interpreterOutput = (Microsoft.Maui.Controls.StackLayout)interpreterOutputFrame.Content;

            // Add the new Frame to the existing StackLayout
            interpreterOutput.Children.Add(interpreterCodeFrame);

            return Task.CompletedTask; // Indicate that the Task is complete

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

        
        //EXECUTES PYTHON

        static bool isGIFEnabled = false; // Inicializa la variable 

        public async Task<string> ExecuteScriptAsync()
        {

            DependeciesForExecutePython dep = new DependeciesForExecutePython();
            var (pythonPath, scriptPath) = await dep.findScriptsForPython();

            ProcessChunksAndJson processChunksAndJson = new ProcessChunksAndJson(); //initializes processChunksAndJson


            Debug.WriteLine($"ExecuteScriptAsync called with pythonPath: {pythonPath}, scriptPath: {scriptPath}, userPrompt: {userPrompt}, apiKey: {apiKey}");  // Monitoring line

            var outputBuilder = new StringBuilder();

            await Task.Run(async () =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = $"{pythonPath}",
                        Arguments = $"\"{scriptPath}\" \"{MainPage.userPrompt}\" \"{apiKey}\" \"{interpreter_model}\"",
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

                        var validJson = processChunksAndJson.ProcessChunk(interpreterChunk);

                        if (!string.IsNullOrEmpty(validJson))
                        {
                            UpdateInterpreterUI(validJson);
                        }

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


        public void UpdateInterpreterUI(string jsonObject) //this function is inside a loop, so we need to be careful to not load it with too much stuff (preferably almost nothing)
        {

            Debug.WriteLine(jsonObject);

            this.Dispatcher.Dispatch(async () =>
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

                    //end-start of message
                    var start_of_message = json["start_of_message"]?.ToObject<bool>();
                    var message = json["message"]?.ToString();
                    var end_of_message = json["end_of_message"]?.ToObject<bool>();

                    //code
                    var end_of_execution = json["end_of_execution"]?.ToObject<bool>();
                    var start_of_code = json["start_of_code"]?.ToObject<bool>();

                    var language = json["language"]?.ToString();
                    var code = json["code"]?.ToString();
                    var executing = json["executing"];
                    var active_line = json["active_line"];
                    var output = json["output"]?.ToString();


                    // start code
                    if (start_of_code == true)
                    {
                        Debug.WriteLine("START OF CODE is true");

                        await AddInterpreterCodeBoxToInterpreterOutputFrame();

                    }
                    
                    if (code != null)
                    {
                        if (isCodeFirstUpdate)
                        {
                            codeLabel.Text = code;  // Set the text to the first message received
                            isCodeFirstUpdate = false;
                            
                            Debug.WriteLine("is code first update FORCE UI");

                            ChatScrollView.ForceLayout();
                            interpreterOutputFrame.ForceLayout();
                            interpreterCodeFrame.ForceLayout();

                        }
                        else
                        {
                            codeLabel.Text += code;  // Append subsequent messages

                        }
                        Debug.WriteLine("codeLAbel: " + codeLabel.Text);

                        if (code.Contains("\n"))
                        {

                             Debug.WriteLine("\n FORCE UI");

                             ChatScrollView.ForceLayout();
                             interpreterOutputFrame.ForceLayout();
                             interpreterCodeFrame.ForceLayout();

                        }
                    }
                    

                    // end code
                    if (end_of_execution == true)
                    {
                        
                         Debug.WriteLine("End of execution FORCE UI");

                         ChatScrollView.ForceLayout();
                         interpreterOutputFrame.ForceLayout();
                         interpreterCodeFrame.ForceLayout();

                    }


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

                            ChatScrollView.ForceLayout();
                            interpreterOutputFrame.ForceLayout();

                            // await Task.Delay(500);  // Optional: give it time to layout if needed
                            var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");
                            await ChatScrollView.ScrollToAsync(0, gridLayout.Height, true);
                        }
                    }

                    if (start_of_message == true)
                    {
                        /*
                         lastLabel = new Label
                         {
                              Text = "",
                              TextColor = Color.FromArgb("#fff"),
                              FontFamily = "Montserrat-Light",
                              IsVisible = false
                         };
                        */

                    }
                    
                    if (end_of_message == true)
                    {
                        
                        this.Dispatcher.Dispatch(async () =>
                        {
                            Debug.WriteLine("ens of message FORCE UI");

                            ChatScrollView.ForceLayout();
                            interpreterOutputFrame.ForceLayout();

                            // await Task.Delay(500);  // Optional: give it time to layout if needed
                            var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");
                            await ChatScrollView.ScrollToAsync(0, gridLayout.Height, true);
                        });

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