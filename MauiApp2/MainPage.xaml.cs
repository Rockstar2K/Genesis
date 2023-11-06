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
                await CloseAllOpenFileFrames(); //CLOSE path UI frames

                await AddInterpreterChatBoxToUI(); 
                await ExecuteScriptAsync();

                //TTSPlayAudioFromText(decodedJson);

                userPrompt = "";

            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString()); //log the exception
            }


        }

        private bool isFileSaved = false;

        public class OpenFileButtonUI
        {
            public Frame frame { get; set; }
            public Label label { get; set; }
            public string labelText { get; set; }
            public Button closeButton { get; set; }
            public Grid grid { get; set; }

            public void InitializeUIComponents()
            {
                Initializelabel();
                InitializeCloseBtn();
                InitializeGrid();
                InitializeFrame();

            }
            private void Initializelabel()
            {
                label = new Label
                {
                    Text = labelText,
                    FontFamily = "Montserrat-Light",
                    FontSize = 10,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#fff"),
                    BackgroundColor = Color.FromArgb("#00000000"),
                    Padding = new Thickness(10),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.Center,
                    WidthRequest = 150,
                    MinimumWidthRequest = 120,
                };
            }

            private void InitializeCloseBtn()
            {
                closeButton = new Button
                {
                    Text = "x",
                    FontSize = 14,
                    BackgroundColor = Color.FromArgb("#fff"),
                    TextColor = Color.FromArgb("#00E0DD"),
                    Padding = 0,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    CornerRadius = 25,
                    BorderWidth = 3,
                    BorderColor = Color.FromArgb("#00E0DD"),
                };
            }

            private void InitializeGrid()
            {
                grid = new Grid
                {
                    ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Auto }
                        },
                    BackgroundColor = Color.FromArgb("#00000000"),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.Center,
                    Padding = new Thickness(0)
                };

                grid.Children.Add(label);
                Grid.SetColumn(label, 0);
                grid.Children.Add(closeButton);
                Grid.SetColumn(closeButton, 1);

            }

            private void InitializeFrame()
            {
                frame = new Frame
                {
                    Content = grid,
                    CornerRadius = 25,
                    HasShadow = false,
                    Padding = 0,
                    Margin = new Thickness(5, 0, 5, 0), // left, top, right, bottom
                    BackgroundColor = Color.FromArgb("#00E0DD"),
                    BorderColor = Color.FromArgb("#00E0DD"),
                    HorizontalOptions = LayoutOptions.End,
                };

            }


        }

        private Dictionary<Frame, string> openFileFrames = new Dictionary<Frame, string>();

        //OPEN FILE
        private async void OpenFileButton_Clicked(object sender, EventArgs e)
        {
            // Use PickMultipleAsync to allow multiple file selections
            var results = await FilePicker.PickMultipleAsync();
            FileBoxContainer.HorizontalOptions = LayoutOptions.End; // Align to the end (right)


            // Check if any files are selected
            if (results?.Count() > 0)
            {
                foreach (var result in results)
                {
                    var addfilePath = result.FullPath;

                    //Debug.WriteLine(" add File Path: " + addfilePath);

                    userPrompt += " Added File (Path): " + addfilePath + " ";

                    string fileName = System.IO.Path.GetFileName(addfilePath);

                    var openFileUI = new OpenFileButtonUI
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
                    openFileFrames.Add(frame, addfilePath);


                }
            }
        }

        // Method to handle the close operation from the close button
        private void HandleClose(Frame frame, string addfilePath)
        {
            if (FileBoxContainer.Children.Contains(frame))
            {
                FileBoxContainer.Children.Remove(frame);
                if (!string.IsNullOrEmpty(addfilePath))
                {
                    userPrompt = userPrompt.Replace($" Added File (Path): {addfilePath} ", string.Empty);
                }
            }
        }

        //Method to handle the close of all the frames after User message is sended
        private async Task CloseAllOpenFileFrames()
        {
            foreach (var frame in openFileFrames.Keys.ToList())
            {
                HandleClose(frame, openFileFrames[frame]);
            }
            openFileFrames.Clear(); // Clear the dictionary after closing all frames
        }




        private InterpreterUI currentInterpreterUI;


        //INTERPRETER CHAT UI
        private async Task AddInterpreterChatBoxToUI()
        {
            var interpreterUI = new InterpreterUI();
            currentInterpreterUI = interpreterUI;

            var gridLayout = (Grid)FindByName("ChatLayout");
            gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            interpreterUI.InitializeUIComponents(screenWidth);

            await this.Dispatcher.DispatchAsync(() =>
            {
                gridLayout.Children.Add(interpreterUI.InterpreterFrame);
                Grid.SetRow(interpreterUI.InterpreterFrame, gridLayout.RowDefinitions.Count - 1);
                Grid.SetColumn(interpreterUI.InterpreterFrame, 0);
            });
        }

        private Task AddLabelToInterpreterOutputFrame(InterpreterUI interpreterUI)
        {
            Debug.WriteLine("AddLabelToInterpreterOutputFrame");

            interpreterUI.AnimatedGif.IsVisible = false;
            interpreterUI.ResultLabel.IsVisible = true;

            this.Dispatcher.Dispatch(async () =>
            {
                var interpreterOutput = (StackLayout)interpreterUI.InterpreterFrame.Content;
                interpreterOutput.Children.Add(interpreterUI.ResultLabel);
            });

            return Task.CompletedTask;
        }

        private Frame currentCodeFrame;

        private Label AddInterpreterCodeBoxToInterpreterOutputFrame(InterpreterUI interpreterUI)
        {
            Debug.WriteLine("AddInterpreterCodeBoxToInterpreterOutputFrame");

            Label codeLabel = interpreterUI.CreateCodeLabel();
            Frame codeFrame = interpreterUI.CreateCodeFrameWithLabel(codeLabel);

            currentCodeFrame = codeFrame;

            var interpreterOutput = (StackLayout)interpreterUI.InterpreterFrame.Content;
            interpreterOutput.Children.Add(codeFrame);

            var gridLayout = (Grid)FindByName("ChatLayout");

            this.Dispatcher.Dispatch(async () =>
            {
                if (OperatingSystem.IsWindows())
                {
                    await ChatScrollView.ScrollToAsync(0, gridLayout.Height, true);
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

                        currentCodeLabel = AddInterpreterCodeBoxToInterpreterOutputFrame(currentInterpreterUI);
                        isCodeFirstUpdate = true;

                       // isOutputFirstUpdate = true; //lets see

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

                        currentInterpreterUI.InterpreterFrame.ForceLayout();
                        currentCodeFrame.ForceLayout();

                        ChatScrollView.ForceLayout();
                        //interpreterOutputFrame.ForceLayout();
                        currentCodeFrame.ForceLayout();

                    }

                    if (start_of_message == true)
                    {

                        await AddLabelToInterpreterOutputFrame(currentInterpreterUI);

                        //  isFirstUpdate = false; ??

                        //isOutputFirstUpdate = true;



                    }

                    if (message != null)
                    {
                        if (isFirstUpdate)
                        {
                            currentInterpreterUI.ResultLabel.Text = message;  // Use currentChatBubble.ResultLabel here
                            isFirstUpdate = false;
                        }
                        else
                        {
                            currentInterpreterUI.ResultLabel.Text += message;  // Use currentChatBubble.ResultLabel here
                        }

                        if (message.Contains("\n"))
                        {
                            Debug.WriteLine("MESSAGE CONTAINS /N");

                            currentInterpreterUI.InterpreterFrame.ForceLayout();
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

                        currentInterpreterUI.InterpreterFrame.ForceLayout();
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