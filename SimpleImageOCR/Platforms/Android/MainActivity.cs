using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SimpleImageOCR
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Request camera permissions at runtime for Android 6.0+
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                RequestPermissions(new string[]
                {
                    Android.Manifest.Permission.Camera,
                    Android.Manifest.Permission.WriteExternalStorage,
                    Android.Manifest.Permission.ReadExternalStorage,
                    Android.Manifest.Permission.ReadMediaImages
                }, 0);
            }
        }
    }
}
