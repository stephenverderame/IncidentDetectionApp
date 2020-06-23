using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IncidentDetection.Resources;

namespace IncidentDetection
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {        
        public static Dictionary<string, double> DEFAULT_SETTINGS = new Dictionary<string, double>
        {
            {Settings.SENSITIVITY, 75 }, {Settings.HELP_TIME, 30}, {Settings.CALL_TIME, 90}, {Settings.CALL_NUMBER, 911}, {Settings.MIN_SPEED, 0}
        };
        public SettingsPage()
        {
            InitializeComponent();

        }
        protected override void OnDisappearing()
        {
            Dictionary<string, double> settings = new Dictionary<string, double>();
            settings.Add(Settings.SENSITIVITY, slider.Value);
            settings.Add(Settings.HELP_TIME, countdownSlider.Value);
            settings.Add(Settings.CALL_TIME, callSwitch.IsToggled ? countdownCallSlider.Value : -1);
            settings.Add(Settings.CALL_NUMBER, callSwitch.IsToggled ? Convert.ToDouble(emergencyNumber.Text) : -1);
 //           settings.Add(Settings.MIN_SPEED, Convert.ToDouble(minSpeedEntry.Text));
            Mediator.invoke(this, new SettingsChangedEventArgs() { Settings = settings });
            Store.save(StoreDataType.Settings, Serialize.serializeSettings(settings.ToArray()));
            base.OnDisappearing();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            var settings = Serialize.deserializeSettings(Store.load(StoreDataType.Settings));
            setUISettings(settings);

        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            double gs = 80 - slider.Value;
            if (gs < 5)
                sensitivityType.Text = AppResources.SensitivityFalls;
            else if (gs < 40)
                sensitivityType.Text = AppResources.SensitivityBike;
            else
                sensitivityType.Text = AppResources.SensitivityCar;

            SensitivityLabel.Text = AppResources.Sensitivity + " " + ((int)e.NewValue).ToString();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            bool y = await DisplayAlert(AppResources.Title, AppResources.ResetTitle, AppResources.Reset, AppResources.Cancel);
            if (y)
            {
                setUISettings(DEFAULT_SETTINGS);
            }
        }
        private void setUISettings(Dictionary<string, double> settings)
        {
            if (settings == null) settings = DEFAULT_SETTINGS;
            try
            {
                slider.Value = settings[Settings.SENSITIVITY];
                countdownSlider.Value = settings[Settings.HELP_TIME];
                double call = settings[Settings.CALL_TIME];
                if (call == -1) callSwitch.IsToggled = false;
                else
                {
                    callSwitch.IsToggled = true;
                    countdownCallSlider.Value = call;
                    emergencyNumber.Text = Convert.ToString(settings[Settings.CALL_NUMBER]);
                    CancelCallLabel.Text = AppResources.CancelCallTime + " " + ((int)settings[Settings.CALL_TIME]);
                    CancelLabel.Text = AppResources.CancelTime + " " + ((int)call);
                }
                SensitivityLabel.Text = AppResources.Sensitivity + " " + ((int)slider.Value).ToString();
//                minSpeedEntry.Text = Convert.ToString(settings[Settings.MIN_SPEED]);
            }
            catch (Exception e)
            {

            }
        }

        private void CountdownSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            CancelLabel.Text = AppResources.CancelTime + " " + ((int)e.NewValue);
        }

        private void CountdownCallSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            CancelCallLabel.Text = AppResources.CancelCallTime + " " + ((int)e.NewValue);
        }
    }
    public struct Settings
    {
        public const string HELP_TIME = "helpTime";
        public const string SENSITIVITY = "sensitivity";
        public const string CALL_TIME = "911Time";
        public const string CALL_NUMBER = "911Num";
        public const string MIN_SPEED = "minSpeed";
    }
}