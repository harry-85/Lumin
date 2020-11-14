using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using HeliosClockCommon.Messages;
using TouchEffect.Android;

namespace HeliosClockApp.Droid
{
    [Activity(Label = "Lumin", Icon = "@mipmap/lumin_icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        static readonly string TAG = typeof(MainActivity).FullName;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Forms.MessagingCenter.Subscribe<StartServerDiscoveryServiceMessage>(new StartServerDiscoveryServiceMessage(), "StartDiscoveryMessage", (s) =>
            {
                Intent i = new Intent(this, typeof(AppDiscoveryService));
                i.AddFlags(ActivityFlags.NewTask);
                StartService(i);
            });

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            var app = new App();

            TouchEffectPreserver.Preserve();
            LoadApplication(app);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            Log.Info(TAG, "Activity is being destroyed; stop the service.");
            base.OnDestroy();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnRestart()
        {
            //StartService(serviceToStart);
            base.OnRestart();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}