using MauiApp2.CustomControls;
using MauiApp2.SCRIPTS;
using Microsoft.Maui;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using static MauiApp2.MainPage;
using CommunityToolkit;


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

        public bool is_executing_code = false;

        //interpreter Chatbox
        private bool isFirstUpdate = true;


        //CODE
        bool isCodeFirstUpdate = true;
        Label currentCodeLabel = null;

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

                if (isFileSaved)
                { //checks if there is a file saved
                    userPrompt += UserInput.Text;
                    isFileSaved = false;

                }
                else
                {
                    userPrompt = UserInput.Text;

                }
                UserInput.Text = "";

                if (!string.IsNullOrEmpty(userPrompt))
                {


                    Debug.WriteLine("User Input memory count: " + memory_count);

                    if (memory_count <= TrimMemoryCS.MaxCharacters) //si es menor se ejecuta addChatBoxes primero
                    {

                        Debug.WriteLine("Memory count LESS than MaxCharachters");

                        await TrimMemoryCS.TrimMemoryFile();
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

        private async void AddChatBoxes() //calls the main functions in order
        {
            try
            {
                Debug.WriteLine("AddChatBoxes");

                var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");
                ScrollView chatScrollView = (ScrollView)FindByName("ChatScrollView");

                await UserChatBoxUI.AddUserChatBoxToUI(gridLayout, chatScrollView, userPrompt);

                await AddInterpreterChatBoxToUI(); //await for this and then call Execute?
                await ExecuteScriptAsync();

                //TTS   //PlayAudioFromText(decodedJson);

                userPrompt = "";

            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString()); //log the exception
            }


        }

        private bool isFileSaved = false;

        //OPEN FILE
        private async void OpenFileButton_Clicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync();

            if (result != null)
            {
                isFileSaved = true;
                var addfilePath = result.FullPath;
                Debug.WriteLine(" add File Path: " + addfilePath);
                userPrompt += " Added File Path: " + addfilePath + " ";

                // Extract file name from the path
                string fileName = System.IO.Path.GetFileName(addfilePath);

                // Create the gradient brush
                var gradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0.5),
                    EndPoint = new Point(1, 0.5)
                };

                // Create the "file box" view
                var fileLabel = new Label
                {
                    Text = fileName,
                    FontFamily = "Montserrat-Light",
                    FontSize = 10,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#fff"),
                    BackgroundColor = Color.FromArgb("#00000000"),
                    Padding = new Thickness(10),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                // Create the close button with a grid container
                var closeButton = new Button
                {
                    Text = "x",
                    FontSize = 10,
                    BackgroundColor = Color.FromArgb("#fff"),
                    TextColor = Color.FromArgb("#fff"),
                    Padding = 0,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    CornerRadius = 25,
                    BorderWidth = 0,
                };

                var buttonContainer = new Grid
                {
                    HeightRequest = 20,
                    WidthRequest = 20,
                    Children = { closeButton },
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };

                // Button click event
                closeButton.Clicked += (s, ev) =>
                {
                    // Remove the file box from the StackLayout
                    //FileBoxContainer.Children.Remove(fileFrame);
                };

                var fileLayout = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = { fileLabel, buttonContainer },
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.Center,
                    BackgroundColor = Color.FromArgb("#00000000"),
                    Margin = new Thickness(5, 10), // Adjust margin for spacing
                    Padding = new Thickness(0), // Remove padding
                    Spacing = 0
                };

                
                // Create the frame that wraps the file layout
                var fileFrame = new Frame
                {
                    Content = fileLayout,
                    CornerRadius = 25,
                    HasShadow = true,
                    Padding = 0,
                    Margin = new Thickness(10, 0, 25, 5),  // left, top, right, bottom
                    BackgroundColor = Color.FromArgb("#00E0DD"),
                    BorderColor = Color.FromArgb("#00000000"),
                    HorizontalOptions = LayoutOptions.End,

                };

                // Add the file frame to the container
                FileBoxContainer.Children.Add(fileFrame);
            }
        }



        public class ChatBubble
        {
            public AnimatedGif AnimatedGif { get; set; }
            public Frame InterpreterFrame { get; set; }

            public Label ResultLabel { get; set; }
            public Label OutputLabel { get; set; }

            public List<Label> CodeLabels { get; set; } = new List<Label>();  // This is a list to hold multiple labels

        }

        private ChatBubble currentChatBubble;


        //INTERPRETER CHAT UI
        private async Task AddInterpreterChatBoxToUI()
        {

            Debug.WriteLine("AddInterpreterChatBoxToUI");

            var chatBubble = new ChatBubble();
            currentChatBubble = chatBubble; 

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
            gradientBrush.GradientStops.Add(new Microsoft.Maui.Controls.GradientStop { Color = Color.FromArgb("#337DFFCF"), Offset = 0 });
            gradientBrush.GradientStops.Add(new Microsoft.Maui.Controls.GradientStop { Color = Color.FromArgb("#7F00E0DD"), Offset = 1 });

            var customShadow = new Microsoft.Maui.Controls.Shadow
            {
                Radius = 10,
                Opacity = 0.6f,
                Brush = new Microsoft.Maui.Controls.SolidColorBrush(new Color(0.690f, 0.502f, 0.718f)),
                Offset = new Point(5, 5)
            };

            chatBubble.AnimatedGif = new AnimatedGif("MauiApp2.Resources.Images.genesis_loading.gif");
            chatBubble.AnimatedGif.WidthRequest = 80;
            chatBubble.AnimatedGif.HeightRequest = 80;

            if (OperatingSystem.IsMacCatalyst())
            {
                chatBubble.InterpreterFrame = new Frame
                {
                    HorizontalOptions = LayoutOptions.Start,
                    HasShadow = true,
                    Shadow = customShadow,
                    Background = gradientBrush,
                    BorderColor = Color.FromRgba("#00000000"),
                    //VerticalOptions = LayoutOptions.FillAndExpand,  // Make it responsive
                    Margin = new Thickness(20, 0, relativeMargin, 0),  // left, top, right, bottom

                };
            }
            else if (OperatingSystem.IsWindows())
            {
                chatBubble.InterpreterFrame = new Frame
                {

                    HasShadow = true,
                    Shadow = customShadow,
                    Background = gradientBrush,
                    BorderColor = Color.FromRgba("#00000000"),
                    //VerticalOptions = LayoutOptions.FillAndExpand,  // Make it responsive
                    Margin = new Thickness(20, 0, relativeMargin, 0),  // left, top, right, bottom

                };
            }



            chatBubble.InterpreterFrame.Content = new Microsoft.Maui.Controls.StackLayout
            {
                Children = { chatBubble.AnimatedGif, chatBubble.ResultLabel }
            };

            // Add the new Frame to the Grid

            this.Dispatcher.Dispatch(async () => //this code seems to work right only in the dispatcher
            {
                gridLayout.Children.Add(chatBubble.InterpreterFrame);
                Microsoft.Maui.Controls.Grid.SetRow(chatBubble.InterpreterFrame, gridLayout.RowDefinitions.Count - 1);
                Microsoft.Maui.Controls.Grid.SetColumn(chatBubble.InterpreterFrame, 0);

                //await Task.Delay(100);
                //await ChatScrollView.ScrollToAsync(0, gridLayout.Height, true); //i dont know why awaiting this doesnt returns (outside the dispatcher)
            });

        }

        private Task AddLabelToInterpreterOutputFrame(ChatBubble chatBubble)
        {

            Debug.WriteLine("AddLabelToInterpreterOutputFrame");

            chatBubble.ResultLabel = new Label
            {
                Text = "",
                TextColor = Color.FromArgb("#121B3F"),
                FontSize = 14,
                FontFamily = "Montserrat-Light", //CONSOLAS TIPOGRAPHY
                IsVisible = false,
                //HorizontalOptions = LayoutOptions.FillAndExpand,  // Make sure the label expands horizontally

            };

            chatBubble.AnimatedGif.IsVisible = false;
            chatBubble.ResultLabel.IsVisible = true;

            this.Dispatcher.Dispatch(async () => //this code seems to work right only in the dispatcher
            {
                var interpreterOutput = (Microsoft.Maui.Controls.StackLayout)chatBubble.InterpreterFrame.Content;
                interpreterOutput.Children.Add(chatBubble.ResultLabel);
            });

            return Task.CompletedTask; // Indicate that the Task is complete

        }
        
        private bool isOutputFirstUpdate = true;
        /*

        private Task AddOutputToInterpreterOutputFrame(ChatBubble chatBubble)
        {

            Debug.WriteLine("AddOutputToInterpreterOutputFrame");

            chatBubble.OutputLabel = new Label
            {
                Text = "",
                TextColor = Color.FromArgb("#F2CFE2"),
                FontSize = 14,
                FontFamily = "Montserrat-Light", //CONSOLAS TIPOGRAPHY
                IsVisible = false
            };

            chatBubble.AnimatedGif.IsVisible = false;
            chatBubble.OutputLabel.IsVisible = true;

            var interpreterOutput = (Microsoft.Maui.Controls.StackLayout)chatBubble.InterpreterFrame.Content;
            interpreterOutput.Children.Add(chatBubble.OutputLabel);


            return Task.CompletedTask; // Indicate that the Task is complete

        }
        */


        private Frame currentCodeFrame;

        private Label AddInterpreterCodeBoxToInterpreterOutputFrame(ChatBubble chatBubble)
        {
            Debug.WriteLine("AddInterpreterCodeBoxToInterpreterOutputFrame");

            // Gradient Brush Configuration
            var gradientBrush = new Microsoft.Maui.Controls.LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5),
                GradientStops =
            {
                new Microsoft.Maui.Controls.GradientStop { Color = Color.FromArgb("#5AFFFFFF"), Offset = 1 },
                new Microsoft.Maui.Controls.GradientStop { Color = Color.FromArgb("#1AFFFFFF"), Offset = 0 }
            }
            };

            // Frame Configuration
            Frame interpreterCodeFrame = new Frame
            {
                Background = gradientBrush,
                BorderColor = Color.FromRgba(255, 255, 255, 0),
                Margin = new Thickness(0, 10, 0, 10),  // left, top, right, bottom
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            currentCodeFrame = interpreterCodeFrame;


            // Label Configuration
            Label codeLabel = new Label
            {
                Text = "",
                TextColor = Color.FromArgb("#121B3F"),
                FontSize = 12,
                FontFamily = "Montserrat-Light"  // CONSOLAS TIPOGRAPHY
            };

            // Assigning Label to Frame
            interpreterCodeFrame.Content = new Microsoft.Maui.Controls.StackLayout
            {
                Children = { codeLabel }
            };

            // Adding Frame to ChatBubble
            var interpreterOutput = (Microsoft.Maui.Controls.StackLayout)chatBubble.InterpreterFrame.Content;
            interpreterOutput.Children.Add(interpreterCodeFrame);

            var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");

            this.Dispatcher.Dispatch(async () => //this code seems to work right only in the dispatcher
            {
                if (OperatingSystem.IsWindows())
                {
                    await ChatScrollView.ScrollToAsync(0, gridLayout.Height, true); //i dont know why awaiting this doesnt returns (outside the dispatcher)
                }

            });

            return codeLabel;
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

        public async Task<string> ExecuteScriptAsync()
        {
            Debug.WriteLine($"ExecuteScriptAsync");  // Monitoring line

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
                        Debug.WriteLine($"The model returned: {interpreterChunk}");  // Monitoring line

                        var validJsonList = processChunksAndJson.ProcessChunk(interpreterChunk);

                        //Debug.WriteLine($"validJsonList: {validJsonList}");  // Monitoring line


                        if (validJsonList != null && validJsonList.Count > 0)
                        {
                            foreach (var validJson in validJsonList)
                            {
                                //Debug.WriteLine("validJson inside for each: " + validJson);
                                UpdateInterpreterUI(validJson);
                            }
                        }

                        outputBuilder.Append(interpreterChunk);

                    }

                }


                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync(); // non-blocking wait

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine("Error/Debug output: " + error);
                }
            });

            return outputBuilder.ToString();
        }


        public void UpdateInterpreterUI(string jsonObject) //this function is inside a loop, so we need to be careful to not load it with too much stuff (preferably almost nothing)
        {

            Debug.WriteLine(jsonObject);

            this.Dispatcher.Dispatch(async () =>
            {

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
                    var start_of_code = json["start_of_code"]?.ToObject<bool>();
                    var code = json["code"]?.ToString();
                    var end_of_code = json["start_of_code"]?.ToObject<bool>();

                    var language = json["language"]?.ToString();
                    var active_line = json["active_line"];

                    //Execution
                    var end_of_execution = json["end_of_execution"]?.ToObject<bool>();
                    var executing = json["executing"];
                    var output = json["output"]?.ToString();


                    // start code
                    if (start_of_code == true)
                    {
                        Debug.WriteLine("START OF CODE is true");

                        currentCodeLabel = AddInterpreterCodeBoxToInterpreterOutputFrame(currentChatBubble);
                        isCodeFirstUpdate = true;

                        isOutputFirstUpdate = true; //lets see

                    }

                    if (code != null)
                    {

                        if (isCodeFirstUpdate)
                        {
                            currentCodeLabel.Text = code;  // Set the text to the first message received
                            isCodeFirstUpdate = false;

                            Debug.WriteLine("is code first update FORCE UI");

                            ChatScrollView.ForceLayout();
                            //interpreterOutputFrame.ForceLayout();
                            //interpreterCodeFrame.ForceLayout();
                            currentCodeFrame.ForceLayout();


                        }
                        else
                        {
                            currentCodeLabel.Text += code;  // Append subsequent messages

                        }
                        Debug.WriteLine("codeLAbel: " + currentCodeLabel.Text);

                        if (code.Contains("\n"))
                        {

                            Debug.WriteLine("\n FORCE UI");

                            ChatScrollView.ForceLayout();
                            //interpreterOutputFrame.ForceLayout();
                            //interpreterCodeFrame.ForceLayout();
                            currentCodeFrame.ForceLayout();


                        }
                    }


                    // end code
                    if (end_of_code == true)
                    {

                        Debug.WriteLine("End of code FORCE UI");

                        currentChatBubble.InterpreterFrame.ForceLayout();
                        currentCodeFrame.ForceLayout();

                        ChatScrollView.ForceLayout();
                        //interpreterOutputFrame.ForceLayout();
                        currentCodeFrame.ForceLayout();

                    }

                    if (start_of_message == true)
                    {

                        await AddLabelToInterpreterOutputFrame(currentChatBubble);

                        //  isFirstUpdate = false; ??

                        isOutputFirstUpdate = true;



                    }

                    if (message != null)
                    {
                        if (isFirstUpdate)
                        {
                            currentChatBubble.ResultLabel.Text = message;  // Use currentChatBubble.ResultLabel here
                            isFirstUpdate = false;
                        }
                        else
                        {
                            currentChatBubble.ResultLabel.Text += message;  // Use currentChatBubble.ResultLabel here
                        }

                        if (message.Contains("\n"))
                        {
                            Debug.WriteLine("MESSAGE CONTAINS /N");

                            currentChatBubble.InterpreterFrame.ForceLayout();
                            ChatScrollView.ForceLayout();

                            var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");

                            if (OperatingSystem.IsWindows())
                            {
                                await ChatScrollView.ScrollToAsync(0, gridLayout.Height, true);

                            }
                        }
                    }


                    if (end_of_message == true)
                    {

                         Debug.WriteLine("ens of message FORCE UI");

                        currentChatBubble.InterpreterFrame.ForceLayout();
                        ChatScrollView.ForceLayout();

                         var gridLayout = (Microsoft.Maui.Controls.Grid)FindByName("ChatLayout");
                        if (OperatingSystem.IsWindows())
                        {
                            await ChatScrollView.ScrollToAsync(0, gridLayout.Height, true);

                        }
                        

                    }


                    if (output != null)
                    {
                        /*
                        if(isOutputFirstUpdate)
                        {
                            await AddOutputToInterpreterOutputFrame(currentChatBubble);
                            currentChatBubble.OutputLabel.Text = output;
                            isOutputFirstUpdate = false;
                        }
                        else
                        {
                            currentChatBubble.OutputLabel.Text += output;

                        }
                        */

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