using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace IncidentDetection
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NumSelectPopup : PopupPage
	{
        class ListItem : INotifyPropertyChanged
        {
            public string number { get; set; }
            public bool selected { get; set; }
            public ListItem(string num, bool selected = false)
            {
                number = num;
                this.selected = selected;
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
        private ObservableCollection<ListItem> listSource;
        private bool cancelFlag;
		public NumSelectPopup(string[] nums)
		{
			InitializeComponent ();
            listSource = new ObservableCollection<ListItem>();
            cancelFlag = false;
            foreach (var n in nums)
            {
                listSource.Add(new ListItem(n));
            }
            contactOptions.ItemsSource = listSource;
        }
        private void okClick(object sender, EventArgs args)
        {
            var popupManager = Rg.Plugins.Popup.Services.PopupNavigation.Instance;
            popupManager.PopAsync(true);
        }
        private void cancelClick(object sender, EventArgs e)
        {
            cancelFlag = true;
            var popupManager = Rg.Plugins.Popup.Services.PopupNavigation.Instance;
            popupManager.PopAsync(true);
        }
        public string getSelectedNumbers()
        {
            if (cancelFlag) return null;
            string selected = "";
            foreach(var n in listSource)
            {
                if (n.selected)
                    selected += n.number + " ";
            }
            return selected;
        }
	}
}