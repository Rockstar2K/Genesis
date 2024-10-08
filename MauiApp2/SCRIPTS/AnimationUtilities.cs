﻿using Microsoft.Maui.Graphics;

public static class AnimationUtilities
{
    // Method to apply dark mode gradient and fade-in effect
    public static async Task ApplyDarkMode(VisualElement element, bool animation = false, uint fadeDuration = 1000)
    {
        Console.WriteLine("Applying DarkMode");

        // Set up the dark mode gradient background
        var darkModeGradient = new LinearGradientBrush
        {
            StartPoint = new Point(-0.25, -0.25),
            EndPoint = new Point(1, 1)
        };
        darkModeGradient.GradientStops.Add(new GradientStop(Color.FromArgb("#A49FF9"), -0.1f));
        darkModeGradient.GradientStops.Add(new GradientStop(Color.FromArgb("#121B3F"), 0.75f));

        // Apply the gradient background
        if (element is Page page)
        {
            page.Background = darkModeGradient;
        }
        else if (element is View view)
        {
            view.Background = darkModeGradient;
        }
        if (animation)
        {
            // Apply the fade-in animation
            element.Opacity = 0;
            await element.FadeTo(1, fadeDuration); // Fades to full opacity over the specified duration
        }

    }

    // Method to apply dark mode gradient and fade-in effect
    public static async Task ApplyLightMode(VisualElement element, bool animation = true, uint fadeDuration = 1000)
    {
        Console.WriteLine("Applying Light Mode");


        // Apply the gradient background
        if (element is Page page)
        {
            page.Background = Color.FromArgb("#F9F9F9");
        }
        else if (element is View view)
        {
            view.Background = Color.FromArgb("#F9F9F9");
        }

        if (animation)
        {
            // Apply the fade-in animation
            element.Opacity = 0;
            await element.FadeTo(1, fadeDuration); // Fades to full opacity over the specified duration
        }
    }
}
