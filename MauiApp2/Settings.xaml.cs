using System.Diagnostics;

namespace MauiApp2;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();
        
	}

    void Save_Button_Pressed(System.Object sender, System.EventArgs e)
    {
    }

    void Button_Pressed(System.Object sender, System.EventArgs e)
    {
        App.Current.MainPage = new NavigationPage(new MainPage());
    }

    private void API_Input_Box_Completed(object sender, EventArgs e)
    {
        MainPage.apiKey = API_Input_Box.Text;
        Debug.WriteLine("API_Input_Box.Text" + API_Input_Box.Text);
    }

    void Switch_Toggled(System.Object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
    {
        bool isToggled = e.Value;
    }
}
