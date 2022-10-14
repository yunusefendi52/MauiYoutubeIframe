using System.Text.Json;
using MauiYoutubeIframe;

namespace MauiYoutubeIframe;

public enum PlayerState
{
    Unstarted = -1,
    Ended,
    Playing,
    Paused,
    Buffering,
    VideoCued,
}

public record YoutubeIframeParams
{
    public int playsinline { get; set; } = 1;
    public int controls { get; set; } = 1;
    public int autoplay { get; set; } = 0;
}

public partial class YoutubeIframe : WebView
{
    public YoutubeIframe()
    {
        MessageHandlers += (s, e) =>
        {
            var @event = e["event"].GetString();
            if (@event == "onPlayerReady")
            {
                OnPlayerReady?.Invoke(this, EventArgs.Empty);
            }
            else if (@event == "onPlayerStateChange")
            {
                var data = (PlayerState)e["data"].GetInt16();
                OnPlayerStateChange?.Invoke(this, data);
            }
            else if (@event == "onPlayerError")
            {
                OnPlayerError?.Invoke(this, EventArgs.Empty);
            }
        };

        UpdateSource();
    }

    public event EventHandler OnPlayerReady;

    public event EventHandler<PlayerState> OnPlayerStateChange;

    public event EventHandler OnPlayerError;

    public static BindableProperty VideoIdProperty = BindableProperty.Create(
        nameof(VideoId),
        typeof(string),
        typeof(YoutubeIframe));

    public string VideoId
    {
        get => (string)GetValue(VideoIdProperty);
        set => SetValue(VideoIdProperty, value);
    }

    public static BindableProperty YoutubeIframeParamsProperty = BindableProperty.Create(
        nameof(YoutubeIframeParams),
        typeof(YoutubeIframeParams),
        typeof(YoutubeIframe));

    public YoutubeIframeParams YoutubeIframeParams
    {
        get => (YoutubeIframeParams)GetValue(YoutubeIframeParamsProperty);
        set => SetValue(YoutubeIframeParamsProperty, value);
    }

    readonly WeakEventManager weakMessageHandlers = new();

    public void SendMessageHandlers(string value)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(value);
        weakMessageHandlers.HandleEvent(this, dict, nameof(MessageHandlers));
    }

    public event EventHandler<Dictionary<string, JsonElement>> MessageHandlers
    {
        add => weakMessageHandlers.AddEventHandler(value);
        remove => weakMessageHandlers.RemoveEventHandler(value);
    }

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(VideoId))
        {
            if (initialLoad)
            {
                UpdateSource();
            }
            else
            {
                LoadVideoById(VideoId);
            }
        }
        else if (propertyName == nameof(YoutubeIframeParams))
            UpdateSource();
    }

    bool initialLoad = true;

    void UpdateSource()
    {
        initialLoad = string.IsNullOrEmpty(VideoId);

        Source = new HtmlWebViewSource
        {
            Html = GetHtml(VideoId, YoutubeIframeParams),
        };
    }

    public void LoadVideoById(string videoId)
    {
        Dispatcher.Dispatch(() =>
        {
            Eval($$"""
                player.loadVideoById({
                videoId: '{{videoId}}'
                });
                """);
        });
    }

    public void PlayVideo()
    {
        Dispatcher.Dispatch(() =>
        {
            Eval("player.playVideo()");
        });
    }

    public void PauseVideo()
    {
        Dispatcher.Dispatch(() =>
        {
            Eval("player.pauseVideo()");
        });
    }

    public void Mute()
    {
        Dispatcher.Dispatch(() => Eval("player.mute()"));
    }

    public void UnMute()
    {
        Dispatcher.Dispatch(() => Eval("player.unMute()"));
    }

    public async Task<bool> IsMuted()
    {
        var isMuted = await EvaluateJavaScriptAsync("player.isMuted()");
        return isMuted == "true";
    }

    public void SetVolume(int value)
    {
        value = Math.Clamp(value, 0, 100);
        Dispatcher.Dispatch(() => Eval($"player.setVolume({value})"));
    }

    public async Task<double> GetVolume()
    {
        var value = await EvaluateJavaScriptAsync("player.getVolume()");
        return double.TryParse(value, out var v) ? v : 0;
    }
}
