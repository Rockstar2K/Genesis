using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Maui.Audio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

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

        public Task ConvertAudio(string inputPath, string outputPath)
        {
            return Task.Run(() =>
            {
                using (var reader = new AudioFileReader(inputPath))
                {
                    var mono = new StereoToMonoProvider16(reader);
                    var monoSampleProvider = mono.ToSampleProvider();
                    var resampler = new WdlResamplingSampleProvider(monoSampleProvider, 16000);
                    WaveFileWriter.CreateWaveFile16(outputPath, resampler);
                }
            });
        }

        public class StereoToMonoProvider16 : IWaveProvider
        {
            private readonly IWaveProvider sourceProvider;

            public StereoToMonoProvider16(IWaveProvider sourceProvider)
            {
                if (sourceProvider.WaveFormat.Channels != 2) throw new ArgumentException("Source must be stereo");
                this.sourceProvider = sourceProvider;
                this.WaveFormat = new WaveFormat(sourceProvider.WaveFormat.SampleRate, 16, 1);
            }

            public WaveFormat WaveFormat { get; }

            public int Read(byte[] buffer, int offset, int count)
            {
                var stereoBuffer = new byte[count];
                var stereoBytesRead = sourceProvider.Read(stereoBuffer, 0, count);
                var monoBytesRead = stereoBytesRead / 2;
                for (int n = 0, o = 0; n < stereoBytesRead; n += 4, o += 2)
                {
                    var left = BitConverter.ToInt16(stereoBuffer, n);
                    var right = BitConverter.ToInt16(stereoBuffer, n + 2);
                    var mono = (short)((left + right) / 2);
                    BitConverter.GetBytes(mono).CopyTo(buffer, o);
                }
                return monoBytesRead;
            }



        }




    }
}
