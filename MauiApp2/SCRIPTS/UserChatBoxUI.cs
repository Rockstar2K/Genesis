using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Threading.Tasks;

namespace MauiApp2.SCRIPTS
{

    public class UserChatBoxUI
    {

        double relativeMargin = (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density) * 0.1; // 10% of screen width

        public Frame frame { get; set; }
        public Label label { get; set; }

        public void InitializeFrame()
        {
            frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#335FB5FF"),
                BorderColor = Color.FromArgb("#335FB5FF"),
                Margin = new Thickness(20, 20, 20, 20),  // left, top, right, bottom
                //VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                Padding = new Thickness(15),

            };
        }

        public void InitializeLabel(string userPrompt)
        {
            label = new Label
            {
                Text = userPrompt,
                FontFamily = "Montserrat-Light",
                TextColor = Color.FromArgb("#121B3F"),
                LineBreakMode = LineBreakMode.NoWrap,
                HorizontalTextAlignment = TextAlignment.End,
                HorizontalOptions = LayoutOptions.FillAndExpand,


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

        public static async Task AddUserChatBoxToUI(VerticalStackLayout stackLayout, ScrollView chatScrollView, string userPrompt)
        {

            var userChatBoxUI = new UserChatBoxUI();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                userChatBoxUI.InitializeLabel(userPrompt);
                userChatBoxUI.InitializeFrame();
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