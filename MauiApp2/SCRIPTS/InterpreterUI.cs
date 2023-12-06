using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp2.CustomControls;
using Microsoft.Maui.Layouts;


namespace MauiApp2.SCRIPTS
{

        public class InterpreterUI
        {

            private double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;

            public AnimatedGif AnimatedGif { get; set; }
            public Frame InterpreterFrame { get; set; }
            public Frame ResultTextFrame { get; set; }
            public Label ResultLabel { get; set; }
            public Label OutputLabel { get; set; }

             public List<Label> CodeLabels { get; set; } = new List<Label>();  // This is a list to hold multiple labels

            public void InitializeUIComponents()
            {
                InitializeAnimatedGif();
                InitializeInterpreterFrame();
                InitializeResultTextFrame();
            }

            private void InitializeAnimatedGif()
            {
                AnimatedGif = new AnimatedGif("MauiApp2.Resources.Images.image001.gif");
                
                if(OperatingSystem.IsWindows())
                {
                AnimatedGif.WidthRequest = 80;
                AnimatedGif.HeightRequest = 80;
                }
                else if (OperatingSystem.IsMacCatalyst())
                {
                AnimatedGif.WidthRequest = 120;
                AnimatedGif.HeightRequest = 120;
            }

        }

            private void InitializeInterpreterFrame()
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
                    Content = new StackLayout
                    {
                        //HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        Spacing = 4,

                        Children = {AnimatedGif, ResultTextFrame}
                    }
                };

                if(OperatingSystem.IsWindows())
                {
                    //InterpreterFrame.HorizontalOptions = LayoutOptions.FillAndExpand;
                } else
                {
                    InterpreterFrame.HorizontalOptions = LayoutOptions.StartAndExpand;
                }


            }

            public Frame InitializeResultTextFrame()
            {
                ResultLabel = new Label
                {
                    Text = "",
                    TextColor = Color.FromArgb("#121B3F"),
                    FontSize = 14,
                    FontFamily = "Montserrat-Light",
                    LineBreakMode = LineBreakMode.WordWrap,

                };
                ResultTextFrame = new Frame
                {
                    Content = ResultLabel,
                    BackgroundColor = Color.FromArgb("00000000"), // Set the background color to transparent using ARGB
                    BorderColor = Color.FromArgb("00000000"),
                    HasShadow = false, // Ensure no shadow is displayed
                    Padding = 0, // Set padding to 0 if you want the Frame to collapse its space
                    Margin = 0, // Set margin to 0 if you want the Frame to collapse its space
                };
            return ResultTextFrame;

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
            public Frame CreateTextFrameWithLabel(Label label) => new Frame
            {
                Background = GetCodeGradientBrush(),
                BorderColor = Color.FromArgb("#00000000"),
                Margin = new Thickness(0, 10, 0, 10),
                Content = new StackLayout { Children = { label } },
                //MinimumHeightRequest = 30
                // MinimumWidthRequest = 100

            };

            //CODE
            public Frame CreateCodeFrameWithLabel(Label label) => new Frame
            {
                Background = GetCodeGradientBrush(),
                BorderColor = Color.FromArgb("#00000000"),
                Margin = new Thickness(0, 10, 0, 10),
                Content = new StackLayout { Children = { label } },
                //MinimumHeightRequest = 30
               // MinimumWidthRequest = 100

            };

            public Label CreateCodeLabel() => new Label
            {
                TextColor = Color.FromArgb("#121B3F"),
                FontSize = 12,
                FontFamily = "Montserrat-Light",
                LineBreakMode = LineBreakMode.WordWrap,
                //HorizontalOptions = LayoutOptions.FillAndExpand,

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
