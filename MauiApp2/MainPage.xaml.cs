using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.TextToSpeech.V1;
using Google.Protobuf;
using Microsoft.Maui.Controls; // Make sure to include the correct namespace for ContentPage

namespace MauiApp2

{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        string Userprompt;

        //reads the crossplatform tts audio file (i think)
        //private readonly IAudioPlayer _audioPlayer;


        public MainPage()// (IAudioPlayer audioPlayer) working on this
        {
            //_audioPlayer = audioPlayer; //we declare the variable we'll use coming from the audioplayer class
            InitializeComponent();
        }


        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);


            InputBox_Completed(sender, e);

        }

        private void InputBox_Completed(System.Object sender, System.EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);

            Userprompt = InputBox.Text;
            Output.Text = Userprompt;

            //await ConvertTextToSpeech(Userprompt);

            RunCommandsAsync();

            Console.Write(Userprompt);

        }

        public async Task RunCommandsAsync()
        {
            // Capture the command from Userprompt
            string command = Userprompt;

            // Initialize a new Process
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C " + command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            // Start the process and asynchronously read the output and error
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            // Wait for the process to exit
            process.WaitForExit();

            // Display the output and error in your UI
            Output.Text = string.IsNullOrEmpty(error) ? output : error;
        }




        /*
        public async Task ConvertTextToSpeech(string text)
        {
            // hard path file
            string credentialsPath = "C:\\Users\\thega\\source\\repos\\MauiApp2\\MauiApp2\\Resources\\Credentials\\high-invest-4da5afee15f3.json";
            
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            TextToSpeechClient client = TextToSpeechClient.Create();

            SynthesizeSpeechResponse response = await client.SynthesizeSpeechAsync(
                input: new SynthesisInput { Text = text },
                voice: new VoiceSelectionParams
                {
                    LanguageCode = "en-US",
                    Name = "en-US-Wavenet-G",
                    SsmlGender = SsmlVoiceGender.Neutral
                },
                audioConfig: new AudioConfig { AudioEncoding = AudioEncoding.Linear16 }
            );
            //esta función crea un audio file a partir de el audio de Google (hay que tener en cuenta que usa un path de nuestro computador)
            using (Stream output = File.Create("C:\\Users\\thega\\Documents\\output.wav"))
            {
                response.AudioContent.WriteTo(output);
            }

            //calls the audio player file with the response
            //_audioPlayer.PlayAudio(response.AudioContent.ToByteArray());

        }
        */
    }

}
