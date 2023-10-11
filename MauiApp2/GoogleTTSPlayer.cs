using NAudio.Midi;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Diagnostics;
using Google.Cloud.TextToSpeech.V1;

namespace MauiApp2
{
    public class GoogleTTSPlayer : IAudioPlayer
    {
        private readonly HttpClient httpClient = new HttpClient();

        public async Task PlayAudioFromText(string text)
        {
            byte[] audioData = await GetAudioData(text);
            PlayAudio(audioData);
        }

        public void PlayAudio(byte[] audioData)
        {
            var filePath = SaveAudioToFile(audioData);
            PlayAudioFile(filePath);
        }

        private string SaveAudioToFile(byte[] audioData)
        {
            var filePath = Path.Combine(FileSystem.CacheDirectory, "audio.wav");
            File.WriteAllBytes(filePath, audioData);

            Debug.WriteLine("filepath: " + filePath);

            return filePath;
        }

        private void PlayAudioFile(string filePath)
        {
#if ANDROID
            // Code for Android
#elif IOS
            // Code for iOS
#elif WINDOWS
            using (var audioOutput = new WaveOutEvent())
            {
                using (var audioFile = new AudioFileReader(filePath))
                {
                    audioOutput.Init(audioFile);
                    audioOutput.Play();
                    while (audioOutput.PlaybackState == PlaybackState.Playing)
                    {
                        Task.Delay(1000).Wait();
                    }
                }
            }
#elif MACCATALYST
            // Code for macOS
#endif
        }



        public async Task<byte[]> GetAudioData(string text)
        {
            string credentialsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MauiApp2", 
            "Resources",
            "Credentials",
            "high-invest-4da5afee15f3.json"
            );

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            // Set up Google Text-to-Speech client
            TextToSpeechClient client = TextToSpeechClient.Create();

            // Perform the Text-to-Speech request
            SynthesizeSpeechResponse response = await client.SynthesizeSpeechAsync(
                input: new SynthesisInput { Text = text },
                voice: new VoiceSelectionParams
                {
                    LanguageCode = "en-US",
                    Name = "en-US-Wavenet-D",
                    SsmlGender = SsmlVoiceGender.Neutral
                },
                audioConfig: new AudioConfig { AudioEncoding = AudioEncoding.Linear16 }
            );

            // Check if audio content is available
            if (response.AudioContent.Length == 0)
            {
                throw new Exception("Audio content is null or empty.");
            }

            // Convert audio content to byte array
            byte[] audioData = response.AudioContent.ToByteArray();

            return audioData;
        }



        private class GoogleTTSResponse
        {
            public string AudioContent { get; set; }
        }
    }
}