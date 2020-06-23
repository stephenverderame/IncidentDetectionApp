using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using IncidentDetection.Resources;

[assembly: Dependency(typeof(IncidentDetection.Droid.BackgroundService))]
namespace IncidentDetection.Droid
{
    //Wrapper of Android Service to run outside the app. Returned by Dependency Service
    public class BackgroundService : IBackgroundService
    {
        private Intent intent;
        private bool running = false;
        private List<Func<bool, bool>> onRunningStateChanged;
        private static int count = 0;

        public BackgroundService()
        {
            onRunningStateChanged = new List<Func<bool, bool>>();
        }

        public bool isServiceRunning()
        {
            return running;
        }

        public void addRunningStateListener(Func<bool, bool> f)
        {
            onRunningStateChanged.Add(f);
        }

        public void startBackgroundService()
        {
            if (count > 0) return;
            running = true;
            foreach (var cb in onRunningStateChanged)
                cb(running);
            intent = new Intent(Android.App.Application.Context, typeof(IncidentService));
            Android.App.Application.Context.StartForegroundService(intent);
            ++count;
        }

        public void stopBackgroundService()
        {
            if (intent != null)
                Android.App.Application.Context.StopService(intent);           
            running = false;
            foreach (var cb in onRunningStateChanged)
                cb(running);
            --count;
        }

        public void stopCountdown()
        {
            if(intent != null)
            {
                Android.App.Application.Context.StopService(intent);
                Android.App.Application.Context.StartForegroundService(intent);
            }
        }
    }

    [Service]
    public class IncidentService : Service
    {
        private BackgroundIncidentCheck task;
        private NotificationManager notifier;

        public string MAIN_ACTIVITY_ACTION { get; private set; } = "Main_Activity";
        //Foreground service channel
        public string CHANNEL_ID_LOW { get; private set; } = "Incident_Detection_Channel_Low";
        //Countdown notification channel
        public string CHANNEL_ID_HIGH { get; private set; } = "Incident_Detection_Channel_High";

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        //Called when service is started
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            System.Diagnostics.Debug.WriteLine("Service start command!");
            notifier = (NotificationManager)GetSystemService(NotificationService);
            //Notification channel for starting the foreground service. No sound or display
            notifier.CreateNotificationChannel(new NotificationChannel(CHANNEL_ID_LOW, AppResources.NotificationTitle, NotificationImportance.Min)
            {
                Description = "Automatic Incident Detection"
            });
            //Notification channel for the countdown
            var chan = new NotificationChannel(CHANNEL_ID_HIGH, AppResources.NotificationTitle, NotificationImportance.Max)
            {
                Description = "Countdown until help is called!"
            };
            notifier.CreateNotificationChannel(chan);

            //intent for when the notification is clicked
            var launchIntent = Android.App.Application.Context.PackageManager.GetLaunchIntentForPackage(Android.App.Application.Context.PackageName);
            launchIntent.AddFlags(ActivityFlags.ClearTop);
            var pi = PendingIntent.GetActivity(Android.App.Application.Context, 0, launchIntent, PendingIntentFlags.UpdateCurrent);


            var foregroundNotification = new NotificationCompat.Builder(this, CHANNEL_ID_LOW).SetContentTitle(AppResources.Title)
                .SetContentText(AppResources.NotificationDesc).SetSmallIcon(Resource.Drawable.iconSmall).SetContentIntent(pi).Build();
            StartForeground(0x8302, foregroundNotification);

            task = BackgroundIncidentCheck.GetInstance((int seconds, bool isCalling911) =>
            {
                //Callback function called every second during countdown
                launchIntent.PutExtra("countdown", true); //adds parameter to intent. Parameter is checked in main activity to see if countdown was active
                pi = PendingIntent.GetActivity(Android.App.Application.Context, 0, launchIntent, PendingIntentFlags.UpdateCurrent);

                //intent for when the cancel button is clicked on the notification
                var cancelIntent = new Intent();
                cancelIntent.SetAction("CANCEL_HELP");
                var cancelPendingIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0, cancelIntent, PendingIntentFlags.CancelCurrent);

                Notification fgNote;
                if (!isCalling911)
                {
                    fgNote = new NotificationCompat.Builder(this, CHANNEL_ID_HIGH).SetContentTitle(AppResources.NotificationTitle)
                .SetContentText(seconds > 0 ? string.Concat(seconds, AppResources.Countdown) : AppResources.Notified).SetSmallIcon(Resource.Drawable.iconSmall)
                .SetContentIntent(pi).AddAction(Resource.Drawable.iconSmall, AppResources.Cancel, cancelPendingIntent).Build();
                }
                else
                {
                    fgNote = new NotificationCompat.Builder(this, CHANNEL_ID_HIGH).SetContentTitle(AppResources.NotificationTitle)
                .SetContentText(seconds > 0 ? string.Concat(seconds, AppResources.CountdownEmergency) : AppResources.NotifiedCall).SetSmallIcon(Resource.Drawable.iconSmall)
                .SetContentIntent(pi).AddAction(Resource.Drawable.iconSmall, AppResources.Cancel, cancelPendingIntent).Build();
                }
                notifier.Notify(0x8302, fgNote); //updates notification
                return 0;
            });
            if (task != null)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    task.begin().Wait();
                });
            }
            return StartCommandResult.Sticky;
        }
        public override void OnDestroy()
        {
            System.Diagnostics.Debug.WriteLine("Cancelling");
            if(task != null) task.stop();
            StopForeground(true);
            base.OnDestroy();
        }
    }
    //Handles the cancel help action of the pending intent assigned to the cancel button on the countdown notification
    //Registered in MainActivity
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new [] { "CANCEL_HELP"})]
    public class CancelBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if(intent.Action == "CANCEL_HELP")
            {
                System.Diagnostics.Debug.WriteLine("Cancel countdown");
                DependencyService.Get<IBackgroundService>().stopCountdown();                
            }
        }
    }
}