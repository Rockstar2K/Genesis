/*
using AVFoundation;
using MauiApp2; // Replace with the actual namespace of your shared code
using Foundation;
using System;

public class iOSAudioPlayer : IAudioPlayer
{
    public void PlayAudio(byte[] audioData)
    {
        NSError error;
        var audioPlayer = new AVAudioPlayer(NSData.FromArray(audioData), "wav", out error);
        audioPlayer.FinishedPlaying += (sender, e) => {
            audioPlayer = null;
        };
        audioPlayer.Play();
    }
}
*/