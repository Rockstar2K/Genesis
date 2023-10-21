using System.Threading;

namespace MauiApp2;

public partial class API_Key_Page : ContentPage
{
	public API_Key_Page()
	{
		InitializeComponent();

    }

    void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        App.Current.MainPage = new NavigationPage(new MainPage());

    }

    void Language_Picker_SelectedIndexChanged(System.Object sender, System.EventArgs e)
    {
        var picker = sender as Picker;
        var selectedValue = (string)picker.SelectedItem;
        Preferences.Set("language", selectedValue);
    }

    async void Continue_Button_Clicked(System.Object sender, System.EventArgs e)
    {
    }
}
