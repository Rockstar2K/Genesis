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
        Shell.SetNavBarIsVisible(this, false);
        LanguagePicker.ItemsSource = new List<string> { "English", "Spanish" };

    }

    void Save_Button_Pressed(System.Object sender, System.EventArgs e)
    {
        if (API_Input_Box.Text != string.Empty && api_Key != string.Empty)
        {
            Preferences.Set("api_key", API_Input_Box.Text);
            Debug.WriteLine("api_key" + API_Input_Box.Text);

        }
    }

    async void Back_Pressed(System.Object sender, System.EventArgs e)
    {
        await Shell.Current.Navigation.PopAsync();
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

        //change GPT 3
        GPT3btn.BackgroundColor = Color.FromArgb("#00E0DD");
        GPT3btn.TextColor = Color.FromArgb("#fff");

        //change GPT 4
        GPT4btn.BackgroundColor = Color.FromArgb("#fff");
        GPT4btn.TextColor = Color.FromArgb("#00E0DD");

        Debug.WriteLine("Interpreter model set to GPT-3.5-turbo");

    }
    void GPT4_Pressed(System.Object sender, System.EventArgs e)
    {

        Preferences.Set("interpreter_model", "gpt-4-turbo");
        Debug.WriteLine("interpreter_model" + interpreter_model);

        //change GPT 4
        GPT4btn.BackgroundColor = Color.FromArgb("#00E0DD");
        GPT4btn.TextColor = Color.FromArgb("#fff");

        //change GPT 4
        GPT3btn.BackgroundColor = Color.FromArgb("#fff");
        GPT3btn.TextColor = Color.FromArgb("#00E0DD");

        Debug.WriteLine("Interpreter model set to GPT-4");
    }

    private void OnLanguagePickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var selectedValue = (string)picker.SelectedItem;
        Preferences.Set("Language", selectedValue);
    }
}
