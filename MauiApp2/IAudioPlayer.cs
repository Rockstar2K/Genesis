

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//this code is for the TTS Audio functionality

namespace MauiApp2
{
    public interface IAudioPlayer
    {
        void PlayAudio(byte[] audioData);
    }
}

