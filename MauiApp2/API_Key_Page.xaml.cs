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
}
