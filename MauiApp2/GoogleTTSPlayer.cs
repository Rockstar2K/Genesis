using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace MauiApp2
{
    public class GoogleTTSPlayer : IAudioPlayer
    {
        private readonly HttpClient httpClient = new HttpClient();

        public void PlayAudio(byte[] audioData)
        {
            var filePath = SaveAudioToFile(audioData);
            PlayAudioFile(filePath);
        }

        private string SaveAudioToFile(byte[] audioData)
        {
            var filePath = Path.Combine(FileSystem.CacheDirectory, "audio.wav");
            File.WriteAllBytes(filePath, audioData);
            return filePath;
        }

        private void PlayAudioFile(string filePath)
        {
            // Code to play the audio file goes here
        }

        public async Task<byte[]> GetAudioData(string text)
        {
            string url = "https://texttospeech.googleapis.com/v1/text:synthesize";
            string apiKey = "YOUR_API_KEY_HERE";  // Replace with your actual API key

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

            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                byte[] audioData = await response.Content.ReadAsByteArrayAsync();
                return audioData;
            }
            else
            {
                throw new Exception($"Failed to get audio data: {response.ReasonPhrase}");
            }
        }


        private string GetGoogleTTSUrl(string text)
        {
            // Construct the URL for Google's TTS service based on the text input.
            // This is a simplified example and may not reflect the actual URL or parameters you need.
            return $"https://texttospeech.googleapis.com/v1/text:synthesize?text={Uri.EscapeDataString(text)}";
        }
    }
}
