using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(IncidentDetection.Droid.AndroidCall))]
namespace IncidentDetection.Droid
{
    public class AndroidCall : IPhoneCall
    {
        public void enableSpeaker()
        {
            var audioManager = (AudioManager)Android.App.Application.Context.GetSystemService(Context.AudioService);
            audioManager.Mode = Mode.InCall;
            audioManager.SpeakerphoneOn = true;
        }

        public void makeCall(string number)
        {
            Intent dialIntent = new Intent(Intent.ActionCall, Android.Net.Uri.Parse("tel:" + number));
            Android.App.Application.Context.StartActivity(dialIntent);

            enableSpeaker();
        }
    }

    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" })]
    public class ResponseListener : BroadcastReceiver
    {
        public event SmsReceivedEvent OnSmsReceived;
        public override void OnReceive(Context context, Intent intent)
        {
            if(intent.Action == Android.Provider.Telephony.Sms.Intents.SmsReceivedAction)
            {
                var msgs = Android.Provider.Telephony.Sms.Intents.GetMessagesFromIntent(intent);
                List<string> msgsFrom = new List<string>();
                foreach(var msg in msgs)
                {
                    msgsFrom.Add(msg.OriginatingAddress);
                }
                OnSmsReceived?.Invoke(msgsFrom.ToArray());
            }
        }
    }
/*    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { TelephonyManager.ActionPhoneStateChanged })]
    public class CallEventHandler : BroadcastReceiver
    {
        public event HangupListener OnCallHangup;
        public override void OnReceive(Context context, Intent intent)
        {
            System.Diagnostics.Debug.WriteLine("Listener received");
            if(intent.Action == TelephonyManager.ActionPhoneStateChanged)
            {
                var state = intent.GetStringExtra(TelephonyManager.ExtraState);
                var prevState = PreferenceManager.GetDefaultSharedPreferences(context).GetString("call_state", TelephonyManager.ExtraStateIdle);
                System.Diagnostics.Debug.WriteLine("Current state: " + state + " Prev state: " + prevState);
                if(state == TelephonyManager.ExtraStateIdle)
                {
                    System.Diagnostics.Debug.WriteLine("Idle!");
                    if (prevState == TelephonyManager.ExtraStateOffhook)
                    {
                        System.Diagnostics.Debug.WriteLine("Hangup!");
                        OnCallHangup?.Invoke();
                    }
                }
                if(state == TelephonyManager.ExtraStateOffhook)
                {                   
                    if (prevState == TelephonyManager.ExtraStateRinging)
                        System.Diagnostics.Debug.WriteLine("Pickup?");
                }
            }
        }
    }*/
}