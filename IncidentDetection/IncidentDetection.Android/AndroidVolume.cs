using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(IncidentDetection.Droid.AndroidVolume))]
namespace IncidentDetection.Droid
{
    public class AndroidVolume : IAudioModify
    {
        public void setVolumeMax()
        {
            var audioManager = (AudioManager)Android.App.Application.Context.GetSystemService(Context.AudioService);
            int maxVolume = audioManager.GetStreamMaxVolume(Android.Media.Stream.Music);
            int currentVolume = audioManager.GetStreamVolume(Stream.Music);
            if (audioManager.IsStreamMute(Stream.Music)) audioManager.SetStreamMute(Stream.Music, false);
            if (currentVolume != maxVolume) audioManager.SetStreamVolume(Stream.Music, maxVolume, VolumeNotificationFlags.PlaySound);
            maxVolume = audioManager.GetStreamMaxVolume(Stream.Notification);
            currentVolume = audioManager.GetStreamVolume(Stream.Notification);
            if (maxVolume != currentVolume) audioManager.SetStreamVolume(Stream.Notification, maxVolume, VolumeNotificationFlags.PlaySound);
            if (audioManager.IsStreamMute(Stream.Notification)) audioManager.SetStreamMute(Stream.Notification, false);
            //           var vibrator = (Vibrator)Application.Context.GetSystemService(Context.VibratorService);
            //            vibrator.Vibrate(VibrationEffect.CreateOneShot(800, 255));
        }
    }
}