namespace MauiApp2;

public partial class Welcome_Page : ContentPage
{
	public Welcome_Page()
	{
		InitializeComponent();
	}

	private void Next_Page_Clicked(object sender, EventArgs e)
	{
        App.Current.MainPage = new NavigationPage(new API_Key_Page());

    }
}
