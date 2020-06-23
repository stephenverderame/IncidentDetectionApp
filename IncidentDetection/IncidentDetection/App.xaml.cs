using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace IncidentDetection
{
    //Cancel icon made by Becris from flaticon.com https://www.flaticon.com/free-icon/forbidden_876215?term=cancel&page=1&position=27
    public partial class App : Application
    {
        private bool countdown, bgRunning;
        public App(bool countdown = false, bool backgroundServiceRunning = false)
        {
            this.countdown = countdown;
            this.bgRunning = backgroundServiceRunning;
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage())
            {
                Title = "Incident Detection"
            };
            if (countdown) ((NavigationPage)MainPage).PushAsync(new IncidentDetectedPage());
        }
        public App() : this(false, false) { }

        protected override void OnStart()
        {
            if (!countdown && !bgRunning && !DependencyService.Get<IBackgroundService>().isServiceRunning())
            {
                DependencyService.Get<IBackgroundService>().startBackgroundService();
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }

}
