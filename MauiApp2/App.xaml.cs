using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using UIKit;
using Application = Microsoft.Maui.Controls.Application;
using System;
using System.Threading.Tasks;
using aimee.Managers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.InteropServices;

namespace MauiApp2;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Routing.RegisterRoute("Settings", typeof(Settings));
        Routing.RegisterRoute("Welcome", typeof(Welcome_Page));
        Routing.RegisterRoute("Chat", typeof(MainPage));


        //Shell.Current.Navigated += OnShellNavigated;
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Check if it's the first time the app is opened
        bool isFirstTime = Preferences.Get("IsFirstTimeOpened", true);

        Shell.Current.Navigated += OnShellNavigated;
        NetworkAccess accessType = Connectivity.Current.NetworkAccess;


        if (isFirstTime)
        {

            if (accessType == NetworkAccess.Internet)
            {
                // Navigate to the specific page for first-time users
                await Shell.Current.GoToAsync("Welcome");

                // Set the flag to false so this page won't be shown again on next launch
                Preferences.Set("IsFirstTimeOpened", false);

            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Network Error",
                    $"You are not " + $"connected and we don't (yet) support " +
                    $"offline models. Connect and re-open the app.", "Ok");

                Application.Current.Quit();
            }
        }
        else
        {
            // Normal app launch
            await Shell.Current.GoToAsync("Chat");

            if (accessType == NetworkAccess.Internet)
            {
                // Connection to internet is available
                bool isUpToDate = await open_interpreter_manager.IsLibraryUpToDateAsync("open-interpreter");
                if (!isUpToDate)
                {
                    await open_interpreter_manager.UpdateLibraryAsync("open-interpreter");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Network Error",
                    $"You are not connected and we don't (yet) support offline " +
                    $"models. Connect and re-open the app.", "Ok");

            }
        }


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
