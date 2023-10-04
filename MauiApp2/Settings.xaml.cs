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

}
