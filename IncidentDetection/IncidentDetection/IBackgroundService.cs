using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;
using Plugin.SimpleAudioPlayer;
using IncidentDetection.Resources;

namespace IncidentDetection
{
    public interface IBackgroundService
    {
        void startBackgroundService();
        void stopBackgroundService();
        bool isServiceRunning();
        void addRunningStateListener(Func<bool, bool> f);
        void stopCountdown();
    }
    public class Util
    {
        public static List<string> getEmergencyNumbers()
        {
            List<Contact> contacts = new List<Contact>();
            var ems = Serialize.deserializeEmergencyContacts(Store.load(StoreDataType.EmergencyList));
            var cms = Serialize.deserializeCustomNumbers(Store.load(StoreDataType.CustomNumbers));
            if (ems != null) contacts.AddRange(ems);
            if (cms != null)
            {
                foreach (var num in cms)
                {
                    if (num.Length >= 7)
                    {
                        contacts.Add(new Contact()
                        {
                            name = "",
                            displayNumber = num,
                            numbers = new[] { num }
                        });
                    }
                }
            }
            List<string> numbers = new List<string>();
            foreach (var n in contacts)
            {
                numbers.AddRange(n.displayNumber.Split(','));
            }
            return numbers;
        }
        public async static Task<string> getPositionLink()
        {
            var location = await getPosition();
            String url = "https://google.com/maps/search/?api=1&query=" + location;
            return location + "  " + url;
        }
        public async static Task<string> getPosition()
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
            return string.Concat(Math.Abs(location.Latitude), (location.Latitude < 0 ? "S" : "N")) +
                "," + Math.Abs(location.Longitude) + (location.Longitude < 0 ? "W" : "E");
        }
    }
    public class BackgroundIncidentCheck
    {
        public bool cancel = false;
        private DateTime lastCollision;
        private int secondsToCall, secondsToCall911;
        private Timer timer, timer911, timerMovement;
        private List<string> numbers = null;
        private List<Contact> contacts = null;
        private Dictionary<string, double> settings = null;
        private Func<int, bool, int> callback;
        private ISimpleAudioPlayer player;
        private double speed;
        private long lastSpeed;
        private static BackgroundIncidentCheck instance;
//        private CancellationTokenSource speechCancellationToken;
        private BackgroundIncidentCheck(Func<int, bool, int> callback)
        {
            this.callback = callback;
            lastCollision = DateTime.Now;
            lastCollision.AddHours(-5);
            numbers = Util.getEmergencyNumbers();
            

        }
        private async void timerCallback(object state)
        {
 //           DependencyService.Get<INotification>().soundEmergencyAlert();
            Vibration.Vibrate(800);
            player.Play();
            callback(secondsToCall, false);
            if (secondsToCall-- <= 0)
            {
                timer.Dispose();
                timer = null;
                await sendAlertSMS();
            }
        }
        private void timer911Callback(object state)
        {
 //           DependencyService.Get<INotification>().soundEmergencyAlert();
            Vibration.Vibrate(1800);
            player.Play();
            callback(secondsToCall911, true);           
            if (secondsToCall911 <= 0)
            {                
                timer911.Dispose();
                timer911 = null;
 /*               speechCancellationToken = new CancellationTokenSource();
                DependencyService.Get<IPhoneListener>().stopListenSms();
                DependencyService.Get<IPhoneListener>().startListenCalls(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Hangup detected!");
                    speechCancellationToken.Cancel();
                    DependencyService.Get<IPhoneListener>().stopListenCalls();
                });*/
                DependencyService.Get<IAudioModify>().setVolumeMax();
                if(settings[Settings.CALL_NUMBER] != -1) //if a number is set
                    DependencyService.Get<IPhoneCall>().makeCall(Convert.ToString(settings[Settings.CALL_NUMBER]));
 //               var msgTimer = new Timer(speakMessage, this, 10000, Timeout.Infinite);
            }
            secondsToCall911 -= 3;
        }
        private async Task<bool> sendAlertSMS()
        {
            string url = await Util.getPositionLink();
            System.Diagnostics.Debug.WriteLine(url);
            if(contacts != null)
            {
                numbers.Clear();
                foreach(var ct in contacts)
                {
                    numbers.AddRange(ct.displayNumber.Split(','));
                }
                contacts = null;
            }
            if (numbers != null && !cancel)
            {                               
                System.Diagnostics.Debug.WriteLine("Sending sms!");
                string smsMsg = AppResources.Message1 + " " + url;
                if (settings[Settings.CALL_TIME] != -1)
                    smsMsg += " " + AppResources.MessageCall1 + " " + (int)settings[Settings.CALL_TIME] + " " + AppResources.MessageCall2;
                DependencyService.Get<ISMS>().sendSms(numbers.ToArray(), smsMsg);
                System.Diagnostics.Debug.WriteLine("Sent sms\n");
            }
            if (timer911 == null && settings[Settings.CALL_TIME] != -1)
            {
                DependencyService.Get<IPhoneListener>().startListenSms(onReceiveSms);
                secondsToCall911 = (int)Math.Round(settings[Settings.CALL_TIME]);
                timer911 = new Timer(timer911Callback, this, 5000, 3000);
                System.Diagnostics.Debug.WriteLine("911 timer started!");
            }
            return true;
        }
        public async Task begin()
        {
            Mediator.subscribe((object sender, ContactChangedEventArgs arg) =>
            {
                contacts = arg.contacts;
            }, true);
            Mediator.subscribe((object sender, SettingsChangedEventArgs e) =>
            {
                settings = e.Settings;
            }, true);
            if (settings == null)
            {
                settings = Serialize.deserializeSettings(Store.load(StoreDataType.Settings));
                if (settings == null) settings = SettingsPage.DEFAULT_SETTINGS;
            }
            player = CrossSimpleAudioPlayer.Current;
            player.Load("AlarmSingle.wav");
            Accelerometer.ReadingChanged += async (object sender, AccelerometerChangedEventArgs args) =>
            {
                if (timer == null && timer911 == null && timerMovement == null && args.Reading.Acceleration.Length() - 1 > 80 - settings[Settings.SENSITIVITY])
                {
 /*                   if(settings[Settings.MIN_SPEED] > 0)
                    {
                        double speed2 = 0;
                        Interlocked.Exchange(ref speed2, speed);
                        if(speed2 > settings[Settings.MIN_SPEED])
                        {
                            long lastSpeed2 = 0;
                            Interlocked.Exchange(ref lastSpeed2, lastSpeed);
                            DateTime d = DateTime.FromBinary(lastSpeed2);
                            if(DateTime.Now.Subtract(d).TotalMinutes < 3)
                            {
                                lastCollision = DateTime.Now;
                                //                   timerMovement = new Timer(checkForMovement, null, 0, 500);

                                secondsToCall = (int)Math.Round(settings[Settings.HELP_TIME]);
                                timer = new Timer(timerCallback, this, 0, 1000);

                                System.Diagnostics.Debug.WriteLine("Bang!");
                            }
                        }
                    }
                    else*/
//                    {
                        lastCollision = DateTime.Now;
                        //                   timerMovement = new Timer(checkForMovement, null, 0, 500);

                        secondsToCall = (int)Math.Round(settings[Settings.HELP_TIME]);
                        timer = new Timer(timerCallback, this, 0, 1000);

                        System.Diagnostics.Debug.WriteLine("Bang!");
//                    }


                }
            };
            Accelerometer.Start(SensorSpeed.Default);
 /*           if(settings[Settings.MIN_SPEED] > 0)
            {
                var t = new Thread(async (object state) =>
                {
                    while (true)
                    {
                        Thread.Sleep(1000);
                        var spd = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
                        Interlocked.Exchange(ref speed, spd.Speed.Value);
                        Interlocked.Exchange(ref lastSpeed, DateTime.Now.ToBinary());
                    }
                });
                t.Start();
            }*/
        }
        public void stop()
        {
            cancel = true;
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            if(timer911 != null)
            {
                timer911.Dispose();
                timer911 = null;
            }
        }
        private async void checkForMovement(object state)
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
            if(location.Speed < 0.8 && timerMovement != null) //probably stopped
            {
                timerMovement.Dispose();
                timerMovement = null;
                secondsToCall = (int)Math.Round(settings[Settings.HELP_TIME]);
                timer = new Timer(timerCallback, this, 0, 1000);

                System.Diagnostics.Debug.WriteLine("Bang!");
            }
            else if(DateTime.Now.Subtract(lastCollision).TotalSeconds > 20 && timerMovement != null)
            {
                timerMovement.Dispose();
                timerMovement = null;
            }
        }
        private void onReceiveSms(string[] senders)
        {
            foreach(var sender in senders)
            {
                string num = "";
                foreach (char c in sender)
                    if (char.IsDigit(c)) num += c;
                foreach(string n in numbers)
                {
                    string num2 = "";
                    foreach (char c in n)
                        if (char.IsDigit(c)) num2 += c;
                    if(num == num2)
                    {
                        if (timer911 != null)
                        {
                            timer911.Dispose();
                            timer911 = null;
                            DependencyService.Get<IPhoneListener>().stopListenSms();
                        }
                    }
                }

            }
        }
/*        private async void speakMessage(object state)
        {
            System.Diagnostics.Debug.WriteLine("Start speech!");
            await TextToSpeech.SpeakAsync("Incident detected at " + await Util.getPosition(), new SpeechOptions() { Volume = 1f }, speechCancellationToken.Token);
            if (!speechCancellationToken.IsCancellationRequested)
            {
                new Timer(speakMessage, state, 5000, Timeout.Infinite);
            }

        }*/
        public static BackgroundIncidentCheck GetInstance(Func<int, bool, int> callback)
        {
            if(instance == null)
            {
                instance = new BackgroundIncidentCheck(callback);
                return instance;
            }
            return null;
        }


    }
}
