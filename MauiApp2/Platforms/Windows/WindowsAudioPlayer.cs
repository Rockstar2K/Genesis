using NAudio.Wave;
using MauiApp2; 
using System.IO;

namespace MauiApp2
{
    public class WindowsAudioPlayer : IAudioPlayer
    {
        public void PlayAudio(byte[] audioData)
        {
            using (var stream = new MemoryStream(audioData))
            using (var waveStream = new WaveFileReader(stream))
            using (var waveOut = new WaveOutEvent())
            {
                waveOut.Init(waveStream);
                waveOut.Play();
            }
        }
    }
}
