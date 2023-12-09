using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace MauiApp2;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
        Routing.RegisterRoute("Settings", typeof(Settings));
        Shell.Current.Navigated += OnShellNavigated;

    }



    private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
    {
        if (Shell.Current.CurrentItem.CurrentItem is IShellSectionController controller &&
            controller.PresentedPage is MainPage mainPage)
        {
            // Now you can call non-static methods on mainPage
            mainPage.ApplyTheme();
        }
    }
}