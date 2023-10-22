namespace MauiApp2.CustomControls;
using SkiaSharp;
using Microsoft.Maui.Controls;
using System;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;

public class AnimatedGif : SKCanvasView
{
    private SKBitmap[] frames;
    private int currentFrame = 0;
    private bool isPlaying = true;
    private bool isAnimating;

    public AnimatedGif(string resourceId)
    {
        LoadFrames(resourceId);
        PrecomputeResizedFrames();
        StartAnimation();
    }

    private void LoadFrames(string resourceId)
    {
        var assembly = typeof(AnimatedGif).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceId);
        if (stream == null)
        {
            throw new InvalidOperationException($"Resource with ID {resourceId} not found.");
        }
        using var codec = SKCodec.Create(stream);
        var frameCount = codec.FrameCount;
        frames = new SKBitmap[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            var info = codec.Info;
            frames[i] = new SKBitmap(info.Width, info.Height);
            codec.GetPixels(info, frames[i].GetPixels(), new SKCodecOptions(i));
        }
    }

    private void PrecomputeResizedFrames(int desiredWidth = 100, int desiredHeight = 100)
    {

        for (int i = 0; i < frames.Length; i++)
        {
            var resizedFrame = frames[i].Resize(new SKImageInfo(desiredWidth, desiredHeight), SKFilterQuality.Medium);
            frames[i] = resizedFrame;
        }
    }

    private void StartAnimation()
    {
        if (isAnimating) return;
        isAnimating = true;

        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(50), () =>
        {
            currentFrame = (currentFrame + 1) % frames.Length;
            InvalidateSurface();  // Triggers a call to OnPaintSurface
            return isAnimating;  // Continue the timer if the animation is still running
        });
    }


    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        var canvas = e.Surface.Canvas;
        canvas.Clear();
        var frame = frames[currentFrame];
        canvas.DrawBitmap(frame, 0, 0);
    }

    public void StopAnimation()
    {
        isAnimating = false;
    }
}
