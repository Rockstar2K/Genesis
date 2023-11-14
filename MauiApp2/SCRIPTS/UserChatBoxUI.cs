using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Threading.Tasks;

namespace MauiApp2.SCRIPTS
{

    public class UserChatBoxUI
    {
        public Frame frame { get; set; }
        public Label label { get; set; }

        public void InitializeFrame(double relativeMargin)
        {
            frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#335FB5FF"),
                BorderColor = Color.FromArgb("#335FB5FF"),
                Margin = new Thickness(relativeMargin, 20, 20, 20),  // left, top, right, bottom
            };
        }

        public void InitializeLabel(string userPrompt)
        {
            label = new Label
            {
                Text = userPrompt,
                FontFamily = "Montserrat-Light",
                TextColor = Color.FromArgb("#121B3F"),
            };
        }

        public void InitializeShadow()
        {
            frame.Shadow = new Shadow
            {
                Brush = Color.FromArgb("000"),
                Offset = new Point(10, 10),
                Radius = 15,
                Opacity = 0.25f
            };
        }
    }

    public static class UserChatBoxLogic
    {
       
        public static async Task AddUserChatBoxToUI(Grid gridLayout, ScrollView chatScrollView, string userPrompt)
        {
            var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            double relativeMargin = screenWidth * 0.1; // 10% of screen width

            var userChatBoxUI = new UserChatBoxUI();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                userChatBoxUI.InitializeLabel(userPrompt);
                userChatBoxUI.InitializeFrame(relativeMargin);
                userChatBoxUI.InitializeShadow();

                userChatBoxUI.frame.Content = userChatBoxUI.label;

                gridLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                int newRow = gridLayout.RowDefinitions.Count - 1;

                gridLayout.Children.Add(userChatBoxUI.frame);
                Grid.SetRow(userChatBoxUI.frame, newRow);
                Grid.SetColumn(userChatBoxUI.frame, 0);
            });

            await Task.Delay(0); // Give time for UI to update

            // Scroll to the bottom of the chat on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (OperatingSystem.IsWindows())
                {
                    chatScrollView.ScrollToAsync(0, gridLayout.Height, true);

                }
            });
        }
    }
}

