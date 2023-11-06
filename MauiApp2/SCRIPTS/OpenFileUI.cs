using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.SCRIPTS
{
    public class OpenFileUI
    {
        public Frame frame { get; set; }
        public Label label { get; set; }
        public string labelText { get; set; }
        public Button closeButton { get; set; }
        public Grid grid { get; set; }

        public void InitializeUIComponents()
        {
            Initializelabel();
            InitializeCloseBtn();
            InitializeGrid();
            InitializeFrame();

        }
        private void Initializelabel()
        {
            label = new Label
            {
                Text = labelText,
                FontFamily = "Montserrat-Light",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#fff"),
                BackgroundColor = Color.FromArgb("#00000000"),
                Padding = new Thickness(10),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 150,
                MinimumWidthRequest = 120,
            };
        }

        private void InitializeCloseBtn()
        {
            closeButton = new Button
            {
                Text = "x",
                FontSize = 14,
                BackgroundColor = Color.FromArgb("#fff"),
                TextColor = Color.FromArgb("#00E0DD"),
                Padding = 0,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 25,
                BorderWidth = 3,
                BorderColor = Color.FromArgb("#00E0DD"),
            };
        }

        private void InitializeGrid()
        {
            grid = new Grid
            {
                ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Auto }
                        },
                BackgroundColor = Color.FromArgb("#00000000"),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(0)
            };

            grid.Children.Add(label);
            Grid.SetColumn(label, 0);
            grid.Children.Add(closeButton);
            Grid.SetColumn(closeButton, 1);

        }

        private void InitializeFrame()
        {
            frame = new Frame
            {
                Content = grid,
                CornerRadius = 25,
                HasShadow = false,
                Padding = 0,
                Margin = new Thickness(5, 0, 5, 0), // left, top, right, bottom
                BackgroundColor = Color.FromArgb("#00E0DD"),
                BorderColor = Color.FromArgb("#00E0DD"),
                HorizontalOptions = LayoutOptions.End,
            };

        }


    }
}
