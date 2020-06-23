using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IncidentDetection
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            DependencyService.Get<IBackgroundService>().addRunningStateListener(changeToggle);
            onOffSwitch.IsToggled = true;
            onOffSwitch.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "IsToggled") {
                    if (!onOffSwitch.IsToggled)
                    {
                        if (DependencyService.Get<IBackgroundService>().isServiceRunning())
                            DependencyService.Get<IBackgroundService>().stopBackgroundService();
                    }
                    else
                    {
                        if (!DependencyService.Get<IBackgroundService>().isServiceRunning())
                            DependencyService.Get<IBackgroundService>().startBackgroundService();
                    }
                }
            };
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            onOffSwitch.IsToggled = DependencyService.Get<IBackgroundService>().isServiceRunning();
        }

        private bool changeToggle(bool running)
        {
            bool old = onOffSwitch.IsToggled;
            onOffSwitch.IsToggled = running;
            return old;
        }

        private async void Ok_Clicked(object sender, EventArgs e)
        {
            bool y = await DisplayAlert("Send Ok?", "This will send a message to your emergency contacts signifying that you are ok. Continue?", "Yes", "Cancel");
            if (y)
            {
                DependencyService.Get<ISMS>().sendSms(Util.getEmergencyNumbers().ToArray(), "This is a message from my incident detection app. I am ok and no further help is needed!");
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ContactPage());
        }

        private async void Help_Clicked(object sender, EventArgs e)
        {
            bool c = await DisplayAlert("Incident Detection", "This will send a text to your emergency contacts. Continue?", "Yes", "Cancel");
            if(c)
            {
                DependencyService.Get<ISMS>().sendSms(Util.getEmergencyNumbers().ToArray(), "This is a message from my incident detection app. Help has been requested at " + Util.getPositionLink());
            }
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SettingsPage());
        }
    }
}