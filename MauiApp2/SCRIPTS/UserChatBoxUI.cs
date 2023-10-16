using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;

namespace MauiApp2.SCRIPTS
{
    public static class UserChatBoxUI
    {
        public static void AddUserChatBoxToUI(VerticalStackLayout stackLayout, string userPrompt)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#F2CFE2"),
                BorderColor = Color.FromArgb("#F2CFE2"),
                Margin = new Thickness(80, 0, 0, 0), //left, top, right, bottom
                // ... other styling ...
                Content = new Label
                {
                    Text = userPrompt,
                    FontFamily = "Montserrat-Light",
                    TextColor = Color.FromArgb("#121B3F"),
                }
            };

            stackLayout.Children.Add(frame);

            frame.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#121B3F")),
                Offset = new Point(0, 5),
                Radius = 15,
                Opacity = 0.6f
            };
        }
    }
}
