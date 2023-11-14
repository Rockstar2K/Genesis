using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp2.CustomControls;


namespace MauiApp2.SCRIPTS
{

        public class InterpreterUI
        {
            public AnimatedGif AnimatedGif { get; set; }
            public Frame InterpreterFrame { get; set; }
            public Label ResultLabel { get; set; }
            public Label OutputLabel { get; set; }

            public List<Label> CodeLabels { get; set; } = new List<Label>();  // This is a list to hold multiple labels

            public void InitializeUIComponents(double screenWidth)
            {
                InitializeAnimatedGif();
                InitializeInterpreterFrame(screenWidth);
                InitializeResultLabel();
            }

            private void InitializeAnimatedGif()
            {
                AnimatedGif = new AnimatedGif("MauiApp2.Resources.Images.genesis_loading.gif");
                AnimatedGif.WidthRequest = 80;
                AnimatedGif.HeightRequest = 80;
            }

            private void InitializeInterpreterFrame(double screenWidth)
            {
                var gradientBrush = GetGradientBrush();
                var customShadow = GetCustomShadow();

                InterpreterFrame = new Frame
                {
                    HasShadow = true,
                    Shadow = customShadow,
                    Background = gradientBrush,
                    BorderColor = Color.FromRgba("#00000000"),
                    Margin = new Thickness(20, 0, screenWidth * 0.05, 0), // Calculate responsive margin
                    Content = new StackLayout() // Assign stack layout here if common for all OS
                    {
                        VerticalOptions = LayoutOptions.StartAndExpand, // Set VerticalOptions for the StackLayout
                        Children = { AnimatedGif, ResultLabel } // Add the AnimatedGif here
                    },
                };

                if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsWindows())
                {
                    // Special handling for MacCatalyst and Windows if needed
                }
            }

            private void InitializeResultLabel()
            {
                ResultLabel = new Label
                {
                    Text = "",
                    TextColor = Color.FromArgb("#121B3F"),
                    FontSize = 14,
                    FontFamily = "Montserrat-Light",
                    IsVisible = false,
                    LineBreakMode = LineBreakMode.WordWrap,
                    HorizontalOptions = LayoutOptions.FillAndExpand

                };

                ResultLabel.SizeChanged += (sender, args) =>
                {
                    // Check the new size and adjust the layout or log for debugging
                    var newSize = (sender as Label)?.Height;
                    System.Diagnostics.Debug.WriteLine($"Label new size: {newSize}");
                };
            }

            private LinearGradientBrush GetGradientBrush()
            {
                var gradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0.5),
                    EndPoint = new Point(1, 0.5)
                };
                gradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb("#337DFFCF"), Offset = 0 });
                gradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb("#7F00E0DD"), Offset = 1 });

                return gradientBrush;
            }

            private Shadow GetCustomShadow()
            {
                var customShadow = new Shadow
                {
                    Radius = 10,
                    Opacity = 0.6f,
                    Brush = new SolidColorBrush(new Color(0.690f, 0.502f, 0.718f)),
                    Offset = new Point(5, 5)
                };

                return customShadow;
            }

            //CODE
            public Frame CreateCodeFrameWithLabel(Label label) => new Frame
            {
                Background = GetCodeGradientBrush(),
                BorderColor = Color.FromArgb("#00000000"),
                Margin = new Thickness(0, 10, 0, 10),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Content = new StackLayout { Children = { label } }
                
            };

            public Label CreateCodeLabel() => new Label
            {
                Text = "",
                TextColor = Color.FromArgb("#121B3F"),
                FontSize = 12,
                FontFamily = "Montserrat-Light",
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.FillAndExpand

            };

            private LinearGradientBrush GetCodeGradientBrush() => new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = Color.FromArgb("#5AFFFFFF"), Offset = 1 },
                    new GradientStop { Color = Color.FromArgb("#1AFFFFFF"), Offset = 0 }
                }
            };

        }

    
}
