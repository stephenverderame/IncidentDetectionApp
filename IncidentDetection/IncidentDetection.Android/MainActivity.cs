using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using Android.Content;
using IncidentDetection.Resources;

namespace IncidentDetection.Droid
{

    [Activity(Label = "IncidentDetection", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, Name = "com.companyname.IncidentDetection.Droid.MainActivity")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity activity;
        public const int PERMISSION_LOCATION_REQUEST = 1;
        public const int PERMISSION_SMS_REQUEST = 2;
        public const int PERMISSION_VIBRATE_REQUEST = 3;
        public const int PERMISSION_RECSMS_REQUEST = 4;
        public const int PERMISSION_MODAUDIO_REQUEST = 4;
        private CancelBroadcastReceiver rec;
        private SmsReceiver smsSentReceiver;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            bool? countdown = Intent?.GetBooleanExtra("countdown", false); //the ? denotes a wrapper around the type that is nullable

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            activity = this;
            var services = ((ActivityManager)Application.Context.GetSystemService(ActivityService)).GetRunningServices(int.MaxValue);
            bool serviceRunning = false;
            foreach(var service in services)
            {
                if(service.Service.ClassName == Java.Lang.Class.FromType(typeof(IncidentService)).CanonicalName)
                {
                    serviceRunning = true;
                    break;
                }
            }
            System.Diagnostics.Debug.WriteLine(string.Concat("Service running: ", serviceRunning));
            LoadApplication(new App(countdown.HasValue && countdown.Value, serviceRunning));
            checkPermissions();
            rec = new CancelBroadcastReceiver();
            RegisterReceiver(rec, new Android.Content.IntentFilter("CANCEL_HELP")); //Registers receiver to listen for cancel click on countdown notification
            smsSentReceiver = new SmsReceiver();
            RegisterReceiver(smsSentReceiver, new IntentFilter(AndroidSms.SMS_SENT)); //Registers receiver to listen for intents fired when sms is sent and delivered
            System.Diagnostics.Debug.WriteLine("Setup receiver");
            RegisterReceiver(smsSentReceiver, new IntentFilter(AndroidSms.SMS_DELIVERED));
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        //Checks if permissions are granted, if not displays a message before requesting them
        private void checkPermissions()
        {
            if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.AccessFineLocation) != (int)Android.Content.PM.Permission.Granted)
            {
                Snackbar.Make(this.FindViewById(Android.Resource.Id.Content), AppResources.LocationPermission, Snackbar.LengthIndefinite)
                    .SetAction(AppResources.OkBtn, new Action<View>(delegate (View obj)
                    {
                        ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation }, PERMISSION_LOCATION_REQUEST);
                    })).Show();
                //               ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation }, PERMISSION_LOCATION_REQUEST);
            }
            if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.SendSms) != (int)Android.Content.PM.Permission.Granted)
            {
                Snackbar.Make(this.FindViewById(Android.Resource.Id.Content), AppResources.SMSPermission, Snackbar.LengthIndefinite)
                    .SetAction(AppResources.OkBtn, new Action<View>(delegate (View obj)
                    {
                        ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.SendSms }, PERMISSION_SMS_REQUEST);
                    })).Show();
 //               ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.SendSms }, PERMISSION_SMS_REQUEST);
            }
            if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.Vibrate) != (int)Android.Content.PM.Permission.Granted)
            {
                Snackbar.Make(this.FindViewById(Android.Resource.Id.Content), AppResources.VibratePermission, Snackbar.LengthIndefinite)
                    .SetAction(AppResources.OkBtn, new Action<View>(delegate (View obj)
                    {
                        ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.Vibrate }, PERMISSION_VIBRATE_REQUEST);
                    })).Show();
            }
            if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReceiveSms) != (int)Android.Content.PM.Permission.Granted)
            {
                Snackbar.Make(this.FindViewById(Android.Resource.Id.Content), AppResources.ReceiveSMSPermission, Snackbar.LengthIndefinite)
                    .SetAction(AppResources.OkBtn, new Action<View>(delegate (View obj)
                    {
                        ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReceiveSms }, PERMISSION_RECSMS_REQUEST);
                    })).Show();
            }
            if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.CallPhone) != (int)Android.Content.PM.Permission.Granted)
            {
                Snackbar.Make(this.FindViewById(Android.Resource.Id.Content), AppResources.CallPermission, Snackbar.LengthIndefinite)
                    .SetAction(AppResources.OkBtn, new Action<View>(delegate (View obj)
                    {
                        ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.CallPhone }, PERMISSION_RECSMS_REQUEST);
                    })).Show();
            }
 /*           if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ModifyAudioSettings) != (int)Android.Content.PM.Permission.Granted)
            {
                Snackbar.Make(this.FindViewById(Android.Resource.Id.Content), AppResources.ModifyAudioPermission, Snackbar.LengthIndefinite)
                    .SetAction("OK", new Action<View>(delegate (View obj)
                    {
                        ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ModifyAudioSettings }, PERMISSION_MODAUDIO_REQUEST);
                    })).Show();
            }*/
        }
    }
}