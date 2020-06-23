using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(IncidentDetection.Droid.AndroidSms))]
namespace IncidentDetection.Droid
{
    public class AndroidSms : ISMS
    {
        public const string SMS_SENT = "SMS_SENT";
        public const string SMS_DELIVERED = "SMS_DELIVERED";
        public async void sendSms(string[] numbers, string msg)
        {
            SmsManager manager = SmsManager.Default;
            System.Diagnostics.Debug.WriteLine("Got sms manager");
            PendingIntent piSend = PendingIntent.GetBroadcast(MainActivity.activity, 0, new Intent(SMS_SENT), 0);
            System.Diagnostics.Debug.WriteLine("Got pending intent");
            PendingIntent piDelivered = PendingIntent.GetBroadcast(MainActivity.activity, 0, new Intent(SMS_DELIVERED), 0);
            var parts = manager.DivideMessage(msg);
            List<PendingIntent> deliveryIntents = new List<PendingIntent>();
            List<PendingIntent> sendIntents = new List<PendingIntent>();
            foreach (var part in parts)
            {
                deliveryIntents.Add(piDelivered);
                sendIntents.Add(piSend);
            }
            foreach (var num in numbers)
            {
                if (num.Length < 7) continue;
                System.Diagnostics.Debug.WriteLine("Sending sms to: " + num);
                try
                {
                    manager.SendMultipartTextMessage(num, null, parts, sendIntents, deliveryIntents);
                }catch (Java.Lang.IllegalArgumentException e)
                {
                    System.Diagnostics.Debug.WriteLine("Bad num: " + num);
                }
                await Task.Delay(1000);
            }
            System.Diagnostics.Debug.WriteLine("Done!");
        }
    }
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "SMS_SENT", "SMS_DELIVERED" })]
    public class SmsReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            string error = "";
            switch ((int)ResultCode)
            {
                case (int)Result.Ok:
                    error = "Ok";
                    break;
                case (int)SmsResultError.NoService:
                    error = "No service";
                    break;
                case (int)SmsResultError.NullPdu:
                    error = "Null Pdu";
                    break;
                case (int)SmsResultError.RadioOff:
                    error = "Radio off";
                    break;
                case (int)SmsResultError.GenericFailure:
                    error = "Generic fail";
                    break;
                default:
                    error = "Other error: " + ResultCode;
                    break;
            }
            if (intent.Action == AndroidSms.SMS_SENT)
            {
                if(ResultCode == Result.Ok)
                    Toast.MakeText(context, Resources.AppResources.SMSSentOk, ToastLength.Short).Show();
                else
                    Toast.MakeText(context, Resources.AppResources.SMSSentFail, ToastLength.Short).Show();
            }
/*            else if (intent.Action == AndroidSms.SMS_DELIVERED)
            {
                Toast.MakeText(context, "Sms delivered: " + error, ToastLength.Long).Show();
            }*/
        }
    }
}