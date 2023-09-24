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
        string prompt;

        public MainPage()
        {
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

            Console.Write("sdfsdf");
        }

        private async void InputBox_Completed(System.Object sender, System.EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);

            prompt = InputBox.Text;
            Output.Text = prompt;

            await ConvertTextToSpeech(prompt);

            Console.Write(prompt);

        }

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
            //esta funciónn crea un audio file a partir de el audio de Google
            using (Stream output = File.Create("C:\\Users\\thega\\Documents\\output.wav"))
            {
                response.AudioContent.WriteTo(output);
            }
        }
    }
}
