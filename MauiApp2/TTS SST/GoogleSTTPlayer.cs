using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using Google.Protobuf;
using Foundation;

namespace MauiApp2
{
    public class GoogleSTTPlayer
    {
        private readonly SpeechClient speechClient;

        public GoogleSTTPlayer()
        {
            SetGoogleCredentials();
            speechClient = SpeechClient.Create();
        }

        
        private void SetGoogleCredentials()
        {
            string credentialsPath = "";

            if (OperatingSystem.IsMacCatalyst())
            {
                //credentialsPath = "/Users/n/Desktop/DesktopItems/Genesis5/MauiApp2/Resources/Credentials/high-invest-4da5afee15f3.json";

                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources");
                var credentialsFilePath = Path.Combine(resourcesPath, "Credentials", "high-invest-4da5afee15f3.json");
                Console.WriteLine(credentialsFilePath);
                Console.WriteLine(AppContext.BaseDirectory);

                if (string.IsNullOrWhiteSpace(credentialsFilePath))
                {
                    Console.WriteLine("Credentials file not found in the app bundle.");
                }
                else
                {
                    Console.WriteLine("Credentials file found: " + credentialsFilePath);
                }

            }
            else if (System.OperatingSystem.IsWindows())
            {
                credentialsPath = "C:\\Users\\thega\\source\\repos\\MauiApp2\\MauiApp2\\Resources\\Credentials\\high-invest-4da5afee15f3.json";  
            }

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
        }
        

        public async Task<string> ConvertSpeechToTextAsync(string convertedAudioFilePath)
        {

            if (string.IsNullOrEmpty(convertedAudioFilePath))
            {
                Debug.WriteLine("recordedAudioFilePath is null or empty.");
                return null;
            }
            if (!File.Exists(convertedAudioFilePath))
            {
                Debug.WriteLine("File does not exist: " + convertedAudioFilePath);
                return null;
            }

            try
            {
                var response = await speechClient.RecognizeAsync(

                    new RecognitionConfig
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = 48000,
                        LanguageCode = "en-US",
                    },
                    RecognitionAudio.FromFile(convertedAudioFilePath)
                );

                var result = response.Results.FirstOrDefault();
                Debug.WriteLine("Result ConvertSTT: " + result);
                return result?.Alternatives.FirstOrDefault()?.Transcript;
            }
            catch (Grpc.Core.RpcException ex)
            {
                Console.WriteLine($"gRPC Error: {ex.Status.Detail}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }

        }
    }
}
