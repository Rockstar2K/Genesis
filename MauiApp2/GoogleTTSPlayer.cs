using NAudio.Midi;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Diagnostics;




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
            string url = "https://texttospeech.googleapis.com/v1/text:synthesize";

            Environment.SetEnvironmentVariable("GOOGLE_TTS_API_KEY", "AIzaSyDHAoi9HfDlf5HHO-EmoUfyO-e1GNL2ZOE");
            string apiKey = Environment.GetEnvironmentVariable("GOOGLE_TTS_API_KEY"); 

            var payload = new
            {
                input = new { text = text },
                voice = new { languageCode = "en-US", name = "en-US-Wavenet-D", ssmlGender = "NEUTRAL" },
                audioConfig = new { audioEncoding = "LINEAR16" }
            };

            string jsonPayload = JsonSerializer.Serialize(payload);

            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{url}?key={apiKey}")
            {
                Content = content
            };

            Debug.WriteLine("Request: " + request);

            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("API Response: " + jsonResponse);

                try
                {
                    var parsedResponse = JsonSerializer.Deserialize<GoogleTTSResponse>(jsonResponse);

                    if (string.IsNullOrEmpty(parsedResponse.AudioContent))
                    {
                        throw new ArgumentNullException("Audio content is null or empty.");
                    }

                    byte[] audioData = Convert.FromBase64String(parsedResponse.AudioContent);
                    return audioData;
                }
                catch (JsonException je)
                {
                    throw new JsonException("Failed to deserialize JSON.", je);
                }
                catch (FormatException fe)
                {
                    throw new FormatException("Failed to convert from Base64.", fe);
                }
            }
            else
            {
                throw new HttpRequestException($"Failed to get audio data: {response.ReasonPhrase}");
            }
        }


        private class GoogleTTSResponse
        {
            public string AudioContent { get; set; }
        }
    }
}
