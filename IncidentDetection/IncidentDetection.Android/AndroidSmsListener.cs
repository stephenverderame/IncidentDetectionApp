using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(IncidentDetection.Droid.AndroidPhoneListener))]
namespace IncidentDetection.Droid
{
    public class AndroidPhoneListener : IPhoneListener
    {
        private ResponseListener listener = null;
//        private CallEventHandler callListener = null;

/*        public void startListenCalls(HangupListener onHangup)
        {
            System.Diagnostics.Debug.WriteLine("Starting call listening");
            if (callListener != null) stopListenCalls();
            callListener = new CallEventHandler();
            callListener.OnCallHangup += onHangup;
            MainActivity.activity.RegisterReceiver(callListener, new IntentFilter(TelephonyManager.ActionPhoneStateChanged));
        }*/

        public void startListenSms(SmsReceivedEvent onSms)
        {
            System.Diagnostics.Debug.WriteLine("Starting sms listening");
            if (listener != null) stopListenSms();
            listener = new ResponseListener();
            listener.OnSmsReceived += onSms;
            MainActivity.activity.RegisterReceiver(listener, new IntentFilter(Android.Provider.Telephony.Sms.Intents.SmsReceivedAction));
        }

/*        public void stopListenCalls()
        {
            if (callListener != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Stopping call listening");
                    MainActivity.activity.UnregisterReceiver(callListener);
                    callListener = null;
                }catch (Java.Lang.Exception e)
                {

                }
            }
        }*/

        public void stopListenSms()
        {
            if (listener != null)
            {
                try
                {
                    MainActivity.activity.UnregisterReceiver(listener);
                    listener = null;
                } catch(Java.Lang.Exception e)
                {

                }
            }
        }
    }
}