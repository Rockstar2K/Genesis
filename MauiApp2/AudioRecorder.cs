using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Maui.Audio;

namespace MauiApp2
{
    public class AudioRecorder
    {
        private readonly IAudioManager audioManager;
        private IAudioRecorder audioRecorder;

        public AudioRecorder(IAudioManager audioManager)
        {
            this.audioManager = audioManager;
            this.audioRecorder = audioManager.CreateRecorder();
        }

        public async Task StartRecordingAsync(string recordedAudioFilePath)
        {
            Debug.WriteLine("Started StartRecordingAsync, recordedAudioFilePath: " + recordedAudioFilePath);

            await audioRecorder.StartAsync(recordedAudioFilePath);
        }


        public async Task<IAudioSource> StopRecordingAsync()
        {
            Debug.WriteLine("Started StopRecordingAsync");
            IAudioSource audioSource = await audioRecorder.StopAsync();
            return audioSource;
        }

       
    }
}
