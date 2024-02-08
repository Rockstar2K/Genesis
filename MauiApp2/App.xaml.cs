using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using UIKit;
using Application = Microsoft.Maui.Controls.Application;

namespace MauiApp2;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Routing.RegisterRoute("Settings", typeof(Settings));
        //Shell.Current.Navigated += OnShellNavigated;
    }

    protected override void OnStart()
    {
        base.OnStart();
        // Now it's safer to subscribe to Shell.Current events.
        Shell.Current.Navigated += OnShellNavigated;
    }


    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = new Window(new AppShell()); // Directly use AppShell as the root page
        window.HandlerChanged += WindowHandlerChanged;
        return window;
    }

    private void WindowHandlerChanged(object sender, EventArgs e)
    {
        if (sender is Window win && win.Handler?.PlatformView is UIWindow uiWin)
        {
            // Make customizations to the UIWindow here
            // Note: Titlebar customizations for Mac Catalyst should be done carefully, as not all title bar properties are available
            uiWin.WindowScene.Titlebar.TitleVisibility = UITitlebarTitleVisibility.Hidden;

        }
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
