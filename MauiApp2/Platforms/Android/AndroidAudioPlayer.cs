/*
using Android.Media;

public void PlayAudio(byte[] audioData)
{
    MediaPlayer player = new MediaPlayer();

    // Create a temporary file to store audio data
    string tempFile = Path.Combine(Path.GetTempPath(), "tempaudio.wav");
    File.WriteAllBytes(tempFile, audioData);

    // Set the data source to the temporary file path
    player.SetDataSource(tempFile);
    player.Prepare();
    player.Start();
}
*/