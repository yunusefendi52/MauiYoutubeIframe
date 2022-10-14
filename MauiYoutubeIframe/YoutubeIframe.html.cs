namespace MauiYoutubeIframe;

public partial class YoutubeIframe
{
    string GetHtml(
        string videoId,
        YoutubeIframeParams iframeParams = null)
    {
        iframeParams ??= new();

        return $$"""
                <!DOCTYPE html>
        <html>

        <head>
            <meta charset='utf-8' />
            <meta http-equiv='X-UA-Compatible' content='IE=edge' />
            <title>Page Title</title>
            <meta name='viewport' content='width=device-width, initial-scale=1' />
            <style>
                html,
                body {
                    margin: 0px;
                    width: 100%;
                    height: 100%;
                }
            </style>
            <script>
                function invokeJsBridge(data){
                {{GetJsBridgeScript()}}
                }
            </script>
        </head>

        <body>
            <div id="ytplayer">Loading</div>

            <script>// Load the IFrame Player API code asynchronously.
                var tag = document.createElement('script');
                tag.src = "https://www.youtube.com/player_api";
                var firstScriptTag = document.getElementsByTagName('script')[0];
                firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);

                // Replace the 'ytplayer' element with an <iframe> and
                // YouTube player after the API code downloads.
                var player;
                function onYouTubePlayerAPIReady() {
                    player = new YT.Player('ytplayer', {
                        width: '100%',
                        videoId: '{{videoId}}',
                        playerVars: {
                            autoplay: {{iframeParams.autoplay}},
                            playsinline: {{iframeParams.playsinline}},
                            controls: {{iframeParams.controls}},
                        },
                        events: {
                            'onReady': onPlayerReady,       
                            'onStateChange': onPlayerStateChange,
                            'onError': onPlayerError,
                        }
                    });
                }

                function onPlayerReady(event) {
                    invokeJsBridge(JSON.stringify({ event: 'onPlayerReady' }));
                }

                function onPlayerStateChange(event) {
                    invokeJsBridge(JSON.stringify({ event: 'onPlayerStateChange', data: event.data }));
                }

                function onPlayerError() {
                    invokeJsBridge(JSON.stringify({ event: 'onPlayerError' }));
                }

            </script>
        </body>

        </html>
        """;
    }

    string GetJsBridgeScript()
    {
#if ANDROID
        return "jsBridge.invokeAction(data);";
#else
        return "window.webkit.messageHandlers.invokeAction.postMessage(data);";
#endif
    }
}

