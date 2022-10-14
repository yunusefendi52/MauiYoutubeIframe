using Android.App;
using Android.Content.PM;
using Android.OS;

namespace MauiYoutubeIframeSample;

[Activity(Theme = "@style/App.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        SetTheme(Resource.Style.App_MainTheme_NoActionBar);

        base.OnCreate(savedInstanceState);
    }
}

