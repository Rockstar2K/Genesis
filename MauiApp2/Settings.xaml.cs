using System.Diagnostics;

namespace MauiApp2;

public partial class Settings : ContentPage
{
    string  api_Key;
    bool    see_code;
    bool    night_mode;
    string  model;

	public Settings()
	{
		InitializeComponent();
	}

    void Save_Button_Pressed(System.Object sender, System.EventArgs e)
    {
        if (API_Input_Box.Text != string.Empty && api_Key != string.Empty)
        {
            Preferences.Set("api_key", API_Input_Box.Text);
        }
    }

    void Button_Pressed(System.Object sender, System.EventArgs e)
    {
        App.Current.MainPage = new NavigationPage(new MainPage());
    }

    private void API_Input_Box_Completed(object sender, EventArgs e)
    {
        MainPage.apiKey = API_Input_Box.Text;
        Debug.WriteLine("API_Input_Box.Text" + API_Input_Box.Text);
        api_Key = API_Input_Box.Text;
    }

    void Code_Switch_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        see_code = e.Value;

        Preferences.Set("see_code", see_code);
    }

    void Night_Mode_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        night_mode = e.Value;

        Preferences.Set("night_mode", night_mode);
    }

    void GPT3_Pressed(System.Object sender, System.EventArgs e)
    {
    }
    void GPT4_Pressed(System.Object sender, System.EventArgs e)
    {
    }
}
