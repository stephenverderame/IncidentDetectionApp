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
	public partial class IncidentDetectedPage : ContentPage
	{
		public IncidentDetectedPage ()
		{
			InitializeComponent ();
		}

        private void Button_Clicked(object sender, EventArgs e)
        {
            DependencyService.Get<IBackgroundService>().stopCountdown();
        }

        private async void Ok_Clicked(object sender, EventArgs e)
        {
            bool y = await DisplayAlert(AppResources.SendOKTitle, AppResources.SendOKDesc, AppResources.YesBtn, AppResources.Cancel);
            if (y)
            {
                DependencyService.Get<ISMS>().sendSms(Util.getEmergencyNumbers().ToArray(), AppResources.OkMessage);
            }
        }

    }
}