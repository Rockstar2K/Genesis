using MauiApp2.CustomControls;
using MauiApp2.SCRIPTS;
using Microsoft.Maui;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using static MauiApp2.MainPage;
using CommunityToolkit;
using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;

namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {

        public static string apiKey     { get; set; }   = Preferences.Get("api_key", "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");
        public bool is_night_mode_on    { get; set; }   = Preferences.Get("theme_dark", true);
        public bool is_code_visible     { get; set; }   = Preferences.Get("see_code", false);
        public static string interpreter_model { get; set; } = Preferences.Get("interpreter_model", "openai/gpt-4-vision-preview");
        private Frame interpreterCodeFrame; // Declare a class-level variable to hold the frame
        //memory
        public long memory_count { get; set; } = Preferences.Get("memory_character_count", (long)0);

        public bool is_executing_code = false;

        //interpreter Chatbox
        private bool isFirstUpdate = true;


        //CODE
        bool isCodeFirstUpdate = true;
        Label currentCodeLabel = null;

        //private GoogleTTSPlayer ttsPlayer = new GoogleTTSPlayer();  // Initializing TTS

        //private GoogleSTTPlayer sttPlayer = new GoogleSTTPlayer(); // Initializing STT
        private AudioRecorder audioRecorder;
        public MainPage()
        {
            InitializeComponent();
            // InitializeAudioRecorder();
            Shell.SetNavBarIsVisible(this, false);
            Connectivity.ConnectivityChanged += OnConnectivityChanged;

            // Register to receive DarkModeMessage
            WeakReferenceMessenger.Default.Register<DarkModeMessage>(this, (recipient, message) =>
            {
                // Call the ApplyDarkMode method
                ApplyTheme();
            });

        }
        public async void ApplyTheme()
        {
            is_night_mode_on = Preferences.Get("theme_dark", true);

            if (is_night_mode_on)
            {
                // Apply dark mode
                await AnimationUtilities.ApplyDarkMode(BackgroundView, false);
                UserInput.TextColor = Color.FromArgb("#FFFFFFFF");
                /*
                AcrilicView1.TintColor = new Color(0, 0, 0, 0); // RGB set to 0 with 0 opacity
                AcrilicView2.TintColor = new Color(0, 0, 0, 0); // RGB set to 0 with 0 opacity
                AcrilicView1.EffectStyle = Xe.AcrylicView.Controls.EffectStyle.Dark;
                AcrilicView2.EffectStyle = Xe.AcrylicView.Controls.EffectStyle.Dark;
                */

            }
            else
            {
                // Apply light mode or other logic
                await AnimationUtilities.ApplyLightMode(BackgroundView, false);
                UserInput.TextColor = Color.FromArgb("#00000000");
                /*
                AcrilicView1.TintColor = Color.FromArgb("#FF");
                AcrilicView2.TintColor = Color.FromArgb("#FF");
                AcrilicView1.EffectStyle = Xe.AcrylicView.Controls.EffectStyle.Light;
                AcrilicView2.EffectStyle = Xe.AcrylicView.Controls.EffectStyle.Light;
                */
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Unregister when the page is no longer visible
            WeakReferenceMessenger.Default.Unregister<DarkModeMessage>(this);
        }

        static string userPrompt;

        //USER PROMPT INPUT
        private async void UserInputBox_Completed(System.Object sender, System.EventArgs e)
        {
            try
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    userPrompt = isFileSaved ? userPrompt + UserInput.Text : UserInput.Text;
                    isFileSaved = false;
                    UserInput.Text = "";

                    if (!string.IsNullOrWhiteSpace(userPrompt))
                    {
                        Debug.WriteLine($"User Input memory count: {memory_count}");

                        await TrimMemoryCS.TrimMemoryFile().ConfigureAwait(false);
                        AddChatBoxes();

                        if (memory_count <= TrimMemoryCS.MaxCharacters)
                        {
                            Debug.WriteLine("Memory count LESS than MaxCharacters");
                        }
                    }
                }
                else
                {
                    // NoInternetFrame.IsVisible = true;
                    await Task.Delay(3000).ConfigureAwait(false);
                    // NoInternetFrame.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                // Handle exceptions (e.g., display a message to the user)
            }
        }


        private async void AddChatBoxes() //calls the main functions in order
        {
            try
            {
                this.Dispatcher.Dispatch(async () =>
                {
                    Debug.WriteLine("AddChatBoxes");

                    var stackLayout = (Microsoft.Maui.Controls.VerticalStackLayout)FindByName("ChatLayout");
                    ScrollView chatScrollView = (ScrollView)FindByName("ChatScrollView");

                    await UserChatBoxLogic.AddUserChatBoxToUI(stackLayout, chatScrollView, userPrompt);

                    //CLOSE path UI frames this function is erasing the filePath
                    //from the prompt before is sended to the API in AddInterpreterChatBoxToUI
                    await CloseAllOpenFileFrames(); 

                    await AddInterpreterChatBoxToUI();

                    await ExecuteScriptAsync();

                    //TTSPlayAudioFromText(decodedJson);

                    userPrompt = "";
                });
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString()); //log the exception
            }


        }


        //private OpenFileUI openFileUI; //Class from another file

        private bool isFileSaved = false;
        private Dictionary<Frame, string> openFileFrames = new Dictionary<Frame, string>();

        //OPEN FILE
        private async void OpenFileButton_Clicked(object sender, EventArgs e)
        {
            // Use PickMultipleAsync to allow multiple file selections
            var results = await FilePicker.PickMultipleAsync();
            FileBoxContainer.HorizontalOptions = LayoutOptions.End; // Align to the end (right)

            // Check if any files are selected
            if (results?.Any() == true)
            {
                foreach (var result in results)
                {
                    var addfilePath = result.FullPath;

                    //Debug.WriteLine(" add File Path: " + addfilePath);

                    userPrompt += addfilePath + " ";

                    string fileName = System.IO.Path.GetFileName(addfilePath);

                    if (fileName.Length > 12)
                    {
                        fileName = fileName.Substring(0, 12) + "...";

                    }

                    this.Dispatcher.Dispatch(() =>
                    {

                        var openFileUI = new OpenFileUI
                        {
                            labelText = fileName
                        };
                        openFileUI.InitializeUIComponents();

                        Frame frame = openFileUI.frame;
                        Label label = openFileUI.label;
                        Button closeButton = openFileUI.closeButton;
                        Grid grid = openFileUI.grid;

                        // Closure to capture the current fileFrame in the loop
                        closeButton.Clicked += (s, ev) => HandleClose(frame, addfilePath);


                        // Add the file frame to the container
                        FileBoxContainer.Children.Insert(0, frame);

                        isFileSaved = true;

                        //add frames to the list (for closure)
                        openFileFrames[frame] = addfilePath; // Use indexer for potential frame update

                    });


                }
            }
        }

        // Modify the HandleClose method to include a flag for updating the userPrompt
        private void HandleClose(Frame frame, string addfilePath, bool updatePrompt = true)
        {
            this.Dispatcher.Dispatch(() =>
            {
                if (FileBoxContainer.Children.Contains(frame))
                {
                    FileBoxContainer.Children.Remove(frame);
                    if (updatePrompt == true && !string.IsNullOrEmpty(addfilePath))
                    {
                        userPrompt = userPrompt.Replace($" Added File (Path): {addfilePath} ", string.Empty);
                    }
                }
            });

        }

        // Modify CloseAllOpenFileFrames to pass 'false' for the updatePrompt parameter
        private async Task CloseAllOpenFileFrames()
        {
            isFileSaved = false;

            this.Dispatcher.Dispatch(() =>
            {
                foreach (var frame in openFileFrames.Keys.ToList())
                {
                    // Pass 'false' to prevent userPrompt from being updated
                    HandleClose(frame, openFileFrames[frame], false);
                }
                openFileFrames.Clear(); // Clear the dictionary after closing all frames
            });

        }


        private InterpreterUI currentInterpreterUI;

        //INTERPRETER CHAT UI
        private async Task AddInterpreterChatBoxToUI()
        {
            await this.Dispatcher.DispatchAsync(() =>
            {
                var interpreterUI = new InterpreterUI();
                currentInterpreterUI = interpreterUI;

                var verticalStackLayout = (VerticalStackLayout)FindByName("ChatLayout");
                interpreterUI.InitializeUIComponents();

                verticalStackLayout.Children.Add(interpreterUI.InterpreterFrame);


            });

        }

        Frame currentTextFrame;

        private Task AddLabelToInterpreterOutputFrame(InterpreterUI interpreterUI)
        {
            Debug.WriteLine("AddLabelToInterpreterOutputFrame");


                interpreterUI.AnimatedGif.IsVisible = false;
                interpreterUI.ResultTextFrame.IsVisible = true;

                Frame currentFrame = interpreterUI.InitializeResultTextFrame();
                currentTextFrame = currentFrame;


                var interpreterOutput = (StackLayout)interpreterUI.InterpreterFrame.Content;
                interpreterOutput.Children.Add(currentTextFrame);

            var stackLayout = (VerticalStackLayout)FindByName("ChatLayout"); // Change to VerticalStackLayout


            return Task.CompletedTask;
        }

        private Frame currentCodeFrame;

        private Label AddInterpreterCodeBoxToInterpreterOutputFrame(InterpreterUI interpreterUI)
        {
            Debug.WriteLine("AddInterpreterCodeBoxToInterpreterOutputFrame");


            Label codeLabel = interpreterUI.CreateCodeLabel();
            Frame codeFrame = interpreterUI.CreateCodeFrameWithLabel(codeLabel);
            this.Dispatcher.Dispatch(() =>
            {
                currentCodeFrame = codeFrame;

                var interpreterOutput = (StackLayout)interpreterUI.InterpreterFrame.Content;
                interpreterOutput.Children.Add(currentCodeFrame);

                //var stackLayout = (VerticalStackLayout)FindByName("ChatLayout"); // Change to VerticalStackLayout

                //scrollToLastChatBox();
            });

            return codeLabel;
        }


        //TTS
        public async void PlayAudioFromText(string text)
        {
            try
            {
                Debug.WriteLine("PlayAudioFromText is called, now it should play: " + text);
                //await ttsPlayer.PlayAudioFromText(text);
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

            await Task.Run( () =>
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

                    while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var interpreterChunk = new string(buffer, 0, charsRead);

                        var validJsonList = processChunksAndJson.ProcessChunk(interpreterChunk);

                        if (validJsonList != null && validJsonList.Count > 0)
                        {
                            foreach (var validJson in validJsonList)
                            {
                                //Debug.WriteLine("validJson inside for each: " + validJson);
                                this.Dispatcher.Dispatch(() =>
                                {
                                    UpdateInterpreterUI(validJson);

                                });
                            }
                        }

                        outputBuilder.Append(interpreterChunk);

                    }

                }


                string error =  process.StandardError.ReadToEnd();
                process.WaitForExit(); // non-blocking wait

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine("Error/Debug output: " + error);
                }
            });

            return outputBuilder.ToString();
        }

        private void scrollToLastChatBox()
        {
            var stackLayout = (Microsoft.Maui.Controls.VerticalStackLayout)FindByName("ChatLayout");
            //ChatScrollView.ScrollToAsync(0, stackLayout.Height, true);

            if (OperatingSystem.IsWindows())
            { 
                ChatScrollView.ScrollToAsync(0, stackLayout.Height, true);
            }
            this.Dispatcher.Dispatch(async () =>
            {
            this.ForceLayout();
            });
        }



        public void UpdateInterpreterUI(string jsonObject) //this function is inside a loop, so we need to be careful to not load it with too much stuff (preferably almost nothing)
        {

            Debug.WriteLine(jsonObject);



                if (string.IsNullOrEmpty(jsonObject))
                {
                    Debug.WriteLine("Text is null or empty updateUI");
                    return;
                }

                try
                {
                    var json = JObject.Parse(jsonObject);

                    var role = json["role"]?.ToString(); //either assistant or computer
                    var type = json["type"]?.ToString(); //type of content. it can be message, code or console
                    var format = json["format"]?.ToString(); //programing language
                    var content = json["content"]?.ToString(); //Well... Content.
                    var start = json["start"]?.ToObject<bool>(); // Start of the block of content, not the output.  
                    var end = json["end"]?.ToObject<bool>();     // End if the block (end of the code part, for instance)



                    // start code
                    if (start == true && type == "code")
                    {
                        is_code_visible = Preferences.Get("see_code", false);
                        //If the code visibility is off...
                        if (!is_code_visible)
                        {
                            AddInterpreterCodeBoxToUI();
                        }
                        else
                        {
                            currentCodeLabel = AddInterpreterCodeBoxToInterpreterOutputFrame(currentInterpreterUI);
                            isCodeFirstUpdate = true;
                        }


                    }

                    else if(content != null && type == "code")
                    {
                        is_code_visible = Preferences.Get("see_code", false);
                        //If the code visibility is off...
                        if (!is_code_visible)
                        {
                            //What to do if visibility is off
                        }
                        else
                        {
                            if (isCodeFirstUpdate)
                            {
                                currentCodeLabel.Text = content;  // Set the text to the first message received
                                isCodeFirstUpdate = false;
                                scrollToLastChatBox();

                            }
                            else
                            {
                                currentCodeLabel.Text += content;  // Append subsequent messages
                            }

                            if (content.Contains("\n"))
                            {
                                //scrollToLastChatBox();
                            }
                        }

                    }

                    // end code
                    else if(end == true && type == "code")
                    {
                        scrollToLastChatBox();
                        currentCodeFrame = null;
                        currentCodeLabel = null;

                    }

                    else if(start == true && type == "message")
                    {
                        AddLabelToInterpreterOutputFrame(currentInterpreterUI);
                        //  isFirstUpdate = false; ??
                        //isOutputFirstUpdate = true;
                    }

                    else if (content != null && type == "message")
                    {
                        if (isFirstUpdate)
                        {
                            currentInterpreterUI.ResultLabel.Text = content;  // Use currentChatBubble.ResultLabel here
                            isFirstUpdate = false;
                        }
                        else
                        {

                            currentInterpreterUI.ResultLabel.Text += content;  // Use currentChatBubble.ResultLabel here
                        }

                        if (content.Contains("\n"))
                        {
                            //scrollToLastChatBox();

                        }
                    }


                    else if (end == true && content == "message")
                    {
                        scrollToLastChatBox();

                    }

                    else if (end == true && type == "console")
                    {
                        currentCodeFrame = null;
                        //If the code visibility is off...
                        if (!is_code_visible)
                        {
                           DeactivateInterpreterCodeBox();
                        }

     
                    }


                }
                catch (JsonReaderException ex)
                {
                    Debug.WriteLine("Json parser exception" + ex.Message);
                    Debug.WriteLine("JSON object: " + jsonObject);

                }

        }

        private void AddInterpreterCodeBoxToUI()
        {
            Debug.WriteLine("AddInterpreterCodeBoxToUI has been called");
            var stackLayout = (VerticalStackLayout)FindByName("ChatLayout");
            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };
            gradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb("#000000"), Offset = 1});
            gradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb("#5FB5FF"), Offset = 0 });
            var customShadow = new Shadow
            {
                Radius = 10,
                Opacity = 0.6f,
                Brush = new SolidColorBrush(new Color(0.690f, 0.502f, 0.718f)),  // #EFCDE1
                Offset = new Point(5, 5)  // Offset of 5 pixels to the right and down
            };
            //Gif Animation
    /*
            var animatedGif2 = new AnimatedGif("MauiApp2.Resources.Images.genesis_loading.gif");
            animatedGif2.WidthRequest = 100;
            animatedGif2.HeightRequest = 100;
            interpreterCodeFrame = new Frame
            {
                HorizontalOptions = LayoutOptions.Start,
                HasShadow = true,
                Shadow = customShadow,
                Background = gradientBrush,
                Margin = new Thickness(20, 20, 80, 0),
                BorderColor = Color.FromRgba(255, 255, 255, 0),
            };
            interpreterCodeFrame.Content = new StackLayout
            {
                Children = { animatedGif2 }
            };
    */
            stackLayout.Children.Add(interpreterCodeFrame);
        }


        private void DeactivateInterpreterCodeBox()
        {
            var stackLayout = (VerticalStackLayout)FindByName("ChatLayout");
            if (interpreterCodeFrame != null)
            {
                stackLayout.Children.Remove(interpreterCodeFrame);
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
