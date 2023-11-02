using Google.Api;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;

namespace MauiApp2.SCRIPTS
{
    public static class UserChatBoxUI
    {
        public static async Task AddUserChatBoxToUI(Grid gridLayout, ScrollView chatScrollView, string userPrompt)
        {

            var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            double relativeMargin = screenWidth * 0.1; // 10% of screen width

            var userInputframe = new Frame
            {
                BackgroundColor = Color.FromArgb("#F2CFE2"),
                BorderColor = Color.FromArgb("#F2CFE2"),
                Margin = new Thickness(relativeMargin, 20, 20, 20),  // left, top, right, bottom

                // ... other styling ...
                Content = new Label
                {
                    Text = userPrompt,
                    FontFamily = "Montserrat-Light",
                    TextColor = Color.FromArgb("#121B3F"),
                }
            };

            gridLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            gridLayout.Children.Add(userInputframe);
            Microsoft.Maui.Controls.Grid.SetRow(userInputframe, gridLayout.RowDefinitions.Count - 1);
            Microsoft.Maui.Controls.Grid.SetColumn(userInputframe, 0);

            userInputframe.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#121B3F")),
                Offset = new Point(0, 5),
                Radius = 15,
                Opacity = 0.6f
            };

            
            await Task.Delay(0);
            chatScrollView.ScrollToAsync(0, gridLayout.Height, true);

        }

    }
}
