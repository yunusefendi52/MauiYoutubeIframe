using System.Text.Json;
using MauiYoutubeIframe;

namespace MauiYoutubeIframeSample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        yt.OnPlayerStateChange += (s, e) =>
        {
            playerState.Text = e.ToString();
        };

        mute.CheckedChanged += (s, e) =>
        {
            if (e.Value)
            {
                yt.Mute();
            }
            else
            {
                yt.UnMute();
            }
        };

        volume.ValueChanged += (s, e) =>
        {
            yt.SetVolume((int)e.NewValue);
        };

        var d = JsonSerializer.Serialize(new YoutubeIframeParams(), new JsonSerializerOptions
        {
            WriteIndented = true,
        });
        editor.Text = d;
    }

    void UpdateParams_Clicked(System.Object sender, System.EventArgs e)
    {
        try
        {
            var p = JsonSerializer.Deserialize<YoutubeIframeParams>(editor.Text);
            yt.YoutubeIframeParams = p;
        }
        catch { }
    }

    void Play_Clicked(System.Object sender, System.EventArgs e)
    {
        yt.PlayVideo();
    }

    void Pause_Clicked(System.Object sender, System.EventArgs e)
    {
        yt.PauseVideo();
    }
}


