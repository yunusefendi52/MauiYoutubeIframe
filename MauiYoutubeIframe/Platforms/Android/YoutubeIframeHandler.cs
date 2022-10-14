using Android.Content;
using Android.Graphics;
using Android.Webkit;
using Java.Interop;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using static Android.Views.ViewGroup;

namespace MauiYoutubeIframe;

public partial class YoutubeIframeHandler : WebViewHandler
{
    protected override Android.Webkit.WebView CreatePlatformView()
    {
        var platformView = new YoutubeIframeWebView(this, Context!)
        {
            LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent)
        };

        platformView.Settings.JavaScriptEnabled = true;
        platformView.Settings.DomStorageEnabled = true;
        platformView.Settings.MediaPlaybackRequiresUserGesture = true;
        platformView.Settings.SetSupportMultipleWindows(true);

        return platformView;
    }
}

public class YoutubeIframeWebView : MauiWebView
{
    public YoutubeIframeWebView(WebViewHandler handler, Context context) : base(handler, context)
    {
        AddJavascriptInterface(new YoutubeIframeJSBridge(handler), "jsBridge");
    }

    bool disposed = false;

    protected override void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            disposed = true;
            RemoveJavascriptInterface("jsBridge");
        }

        base.Dispose(disposing);
    }
}

public class YoutubeIframeJSBridge : Java.Lang.Object
{
    readonly WeakReference<WebViewHandler> weakHandler;

    public YoutubeIframeJSBridge(WebViewHandler handler)
    {
        weakHandler = new WeakReference<WebViewHandler>(handler);
    }

    [JavascriptInterface]
    [Export("invokeAction")]
    public void InvokeAction(string data)
    {
        if (weakHandler.TryGetTarget(out var handler))
        {
            if (handler.VirtualView is YoutubeIframe virtualView)
            {
                virtualView.SendMessageHandlers(data);
            }
        }
    }
}
