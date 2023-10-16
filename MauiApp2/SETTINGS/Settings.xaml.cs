using System.Diagnostics;

namespace MauiApp2;

public partial class Settings : ContentPage
{
    string api_Key;
    bool see_code;
    bool night_mode;
    string interpreter_model;

    public Settings()
    {
        InitializeComponent();
    }

    void Save_Button_Pressed(System.Object sender, System.EventArgs e)
    {
        if (API_Input_Box.Text != string.Empty && api_Key != string.Empty)
        {
            /*
            MainPage.apiKey = API_Input_Box.Text;
            Debug.WriteLine("API_Input_Box.Text" + API_Input_Box.Text);
            api_Key = API_Input_Box.Text;
            */
            Preferences.Set("api_key", API_Input_Box.Text);
            Debug.WriteLine("api_key" + API_Input_Box.Text);

        }
    }

    void Back_Pressed(System.Object sender, System.EventArgs e)
    {
        App.Current.MainPage = new NavigationPage(new MainPage());
    }


    void Code_Switch_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        see_code = e.Value;

        Preferences.Set("True", see_code);
    }

    void Night_Mode_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        night_mode = e.Value;

        Preferences.Set("night_mode", night_mode);
    }

    void GPT3_Pressed(System.Object sender, System.EventArgs e)
    {

        Preferences.Set("interpreter_model", "gpt-3.5-turbo");

    }
    void GPT4_Pressed(System.Object sender, System.EventArgs e)
    {

        Preferences.Set("interpreter_model", "gpt-4");

    }
}
