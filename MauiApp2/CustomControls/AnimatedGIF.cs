namespace MauiApp2.CustomControls;
using SkiaSharp;
using Microsoft.Maui.Controls;
using System;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;


public class AnimatedGif : SKCanvasView
{
    private SKBitmap[] frames;
    private int currentFrame = 0;
    private bool isPlaying = true;
    private bool isAnimating;

    public AnimatedGif(string resourceId) : base()
    {
        LoadFrames(resourceId);
        PrecomputeResizedFrames();
        StartAnimation();
        // Set the view to expand to fill the space, and to scale the image proportionally
        this.HorizontalOptions = LayoutOptions.FillAndExpand;
        this.VerticalOptions = LayoutOptions.FillAndExpand;
    }


    private void LoadFrames(string resourceId)
    {
        var assembly = typeof(AnimatedGif).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceId);
        if (stream == null)
        {
            Console.WriteLine($"Resource with ID {resourceId} not found.");
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

    private void PrecomputeResizedFrames(int desiredWidth = 225, int desiredHeight = 225)
    {

        if (OperatingSystem.IsWindows())
        {
            desiredWidth = 100;
            desiredHeight = 100;
        }

        for (int i = 0; i < frames.Length; i++)
        {
            var resizedFrame = frames[i].Resize(new SKImageInfo(desiredWidth, desiredHeight), SKFilterQuality.Low);
            frames[i] = resizedFrame;
        }
    }

    private void StartAnimation()
    {
        if (isAnimating) return;
        isAnimating = true;

        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(15), () =>
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

        var info = e.Info; // Information about the surface/canvas including size
        var frame = frames[currentFrame];

        // Calculate aspect ratio of the frame
        float frameAspectRatio = frame.Width / (float)frame.Height;
        // Calculate the scaling factor to maintain aspect ratio
        float scale = Math.Min(info.Width / (float)frame.Width, info.Height / (float)frame.Height);

        // New width and height based on the scale factor
        float scaledWidth = frame.Width * scale;
        float scaledHeight = frame.Height * scale;

        // Calculate the X and Y positions to center the frame
        float x = (info.Width - scaledWidth) / 2;
        float y = (info.Height - scaledHeight) / 2;

        // Create a rectangle that represents the destination for the frame
        SKRect destRect = new SKRect(x, y, x + scaledWidth, y + scaledHeight);

        // Draw the bitmap to the canvas, scaling it to fit the destination rectangle
        canvas.DrawBitmap(frame, destRect);
    }

    public void StopAnimation()
    {
        isAnimating = false;
    }
}
