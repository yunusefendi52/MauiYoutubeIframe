#if IOS || MACCATALYST

#nullable enable

using System.Drawing;
using System.Reflection.Metadata;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using WebKit;
using static CoreFoundation.DispatchSource;

namespace MauiYoutubeIframe;

public partial class YoutubeIframeHandler : WebViewHandler
{
    protected override WKWebView CreatePlatformView()
    {
        var p = new YoutubeIframeWKWebView(this, new WKWebViewConfiguration()
        {
            AllowsInlineMediaPlayback = true,
            MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.All,
        });
        return p;
    }
}

// TODO: Does not have configuration constructor in 6.0.541 https://github.com/dotnet/maui/blob/6.0.541/src/Core/src/Platform/iOS/MauiWKWebView.cs
// but it's there in main branch
public partial class YoutubeIframeWKWebView : WKWebView, IWebViewDelegate
{
    static WKProcessPool? SharedPool;

    string? _pendingUrl;
    readonly WeakReference<WebViewHandler> _handler;

    public YoutubeIframeWKWebView(WebViewHandler handler, WKWebViewConfiguration configuration)
        : base(RectangleF.Empty, configuration)
    {
        _ = handler ?? throw new ArgumentNullException("handler");
        _handler = new WeakReference<WebViewHandler>(handler);

        InitConfig();

        BackgroundColor = UIColor.Clear;
        AutosizesSubviews = true;

        NavigationDelegate = new MauiWebViewNavigationDelegate(handler);
    }

    //public string? CurrentUrl =>
    //    Url?.AbsoluteUrl?.ToString();

    //public override void MovedToWindow()
    //{
    //    base.MovedToWindow();

    //    if (!string.IsNullOrWhiteSpace(_pendingUrl))
    //    {
    //        var closure = _pendingUrl;
    //        _pendingUrl = null;

    //        // I realize this looks like the worst hack ever but iOS 11 and cookies are super quirky
    //        // and this is the only way I could figure out how to get iOS 11 to inject a cookie 
    //        // the first time a WkWebView is used in your app. This only has to run the first time a WkWebView is used 
    //        // anywhere in the application. All subsequents uses of WkWebView won't hit this hack
    //        // Even if it's a WkWebView on a new page.
    //        // read through this thread https://developer.apple.com/forums/thread/99674
    //        // Or Bing "WkWebView and Cookies" to see the myriad of hacks that exist
    //        // Most of them all came down to different variations of synching the cookies before or after the
    //        // WebView is added to the controller. This is the only one I was able to make work
    //        // I think if we could delay adding the WebView to the Controller until after ViewWillAppear fires that might also work
    //        // But we're not really setup for that
    //        // If you'd like to try your hand at cleaning this up then UI Test Issue12134 and Issue3262 are your final bosses
    //        InvokeOnMainThread(async () =>
    //        {
    //            await Task.Delay(500);
    //            if (_handler.TryGetTarget(out var handler))
    //            //await handler.FirstLoadUrlAsync(closure);
    //            {
    //                LoadRequest(new(new NSUrl("https://google.com")));
    //            }
    //        });
    //    }
    //}

    //[Export("webView:didFinishNavigation:")]
    //public async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
    //{
    //    var url = CurrentUrl;

    //    if (url == null || url == $"file://{NSBundle.MainBundle.BundlePath}/")
    //        return;

    //    if (_handler.TryGetTarget(out var handler))
    //    { //await handler.ProcessNavigatedAsync(url);
    //    }
    //}

    public void LoadHtml(string? html, string? baseUrl)
    {
        if (html != null)
            LoadHtmlString(html, baseUrl == null ? new NSUrl(NSBundle.MainBundle.BundlePath, true) : new NSUrl(baseUrl, true));
    }

    public void LoadUrl(string? url)
    {
        var uri = new Uri(url ?? string.Empty);
        var safeHostUri = new Uri($"{uri.Scheme}://{uri.Authority}", UriKind.Absolute);
        var safeRelativeUri = new Uri($"{uri.PathAndQuery}{uri.Fragment}", UriKind.Relative);
        NSUrlRequest request = new NSUrlRequest(new NSUrl(new Uri(safeHostUri, safeRelativeUri).AbsoluteUri));

        LoadRequest(request);
    }

    //// https://developer.apple.com/forums/thread/99674
    //// WKWebView and making sure cookies synchronize is really quirky
    //// The main workaround I've found for ensuring that cookies synchronize 
    //// is to share the Process Pool between all WkWebView instances.
    //// It also has to be shared at the point you call init
    //public static WKWebViewConfiguration CreateConfiguration()
    //{
    //    var config = new WKWebViewConfiguration();

    //    if (SharedPool == null)
    //        SharedPool = config.ProcessPool;
    //    else
    //        config.ProcessPool = SharedPool;

    //    return config;
    //}
}

public partial class YoutubeIframeWKWebView : IWKScriptMessageHandler
{
    WKUserContentController? userController;

    void InitConfig()
    {
        userController = Configuration.UserContentController;
        userController.AddScriptMessageHandler(this, "invokeAction");
    }

    public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
    {
        if (_handler.TryGetTarget(out var handler))
        {
            if (handler.VirtualView is YoutubeIframe virtualView)
            {
                var data = message.Body.ToString();
                virtualView.SendMessageHandlers(data);
            }
        }
    }

    bool disposed = false;

    protected override void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            disposed = true;

            userController?.RemoveScriptMessageHandler("invokeAction");
        }

        base.Dispose(disposing);
    }
}

#endif