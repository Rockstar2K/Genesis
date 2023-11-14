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
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(10),


            };
        }

        public void InitializeLabel(string userPrompt)
        {
            label = new Label
            {
                Text = userPrompt,
                FontFamily = "Montserrat-Light",
                TextColor = Color.FromArgb("#121B3F"),
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.FillAndExpand

            };

            label.SizeChanged += (sender, args) =>
            {
                // Check the new size and adjust the layout or log for debugging
                var newSize = (sender as Label)?.Height;
                System.Diagnostics.Debug.WriteLine($"Label new size: {newSize}");
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

            frame.SizeChanged += (sender, args) =>
            {
                // Check the new size and adjust the layout or log for debugging
                var newWidth = (sender as Frame)?.Width;
                var newHeight = (sender as Frame)?.Height;
                System.Diagnostics.Debug.WriteLine($"Frame new size: Width={newWidth}, Height={newHeight}");
            };
        }
    }

    public static class UserChatBoxLogic
    {

        public static async Task AddUserChatBoxToUI(VerticalStackLayout stackLayout, ScrollView chatScrollView, string userPrompt)
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

                stackLayout.Children.Add(userChatBoxUI.frame);
            });

            await Task.Delay(0); // Give time for UI to update

            // Scroll to the bottom of the chat on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (OperatingSystem.IsWindows())
                {
                    chatScrollView.ScrollToAsync(0, stackLayout.Height, true);
                }
            });
        }

    }
}

