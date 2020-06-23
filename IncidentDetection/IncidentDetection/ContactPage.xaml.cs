using IncidentDetection.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace IncidentDetection
{
    public partial class ContactPage : ContentPage
    {
        private ObservableCollection<Contact> contactListSource;
        //trie that organizes indexes in contactListSource based on name
        private TrieCollection<int> allContacts;
        private List<string> customNumbers;
        private bool dirtyFlag;
        public ContactPage()
        {
            InitializeComponent();
            allContacts = new TrieCollection<int>();
            customNumbers = new List<string>();
            Task.Run(() =>
            {
                populateLists();
            });
            dirtyFlag = false;
        }
        protected override void OnDisappearing()
        {
            if (dirtyFlag) save();
            base.OnDisappearing();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (DependencyService.Get<IBackgroundService>().isServiceRunning())
            {
                DependencyService.Get<IBackgroundService>().stopBackgroundService();
            }
            else
            {
                DependencyService.Get<IBackgroundService>().startBackgroundService();
            }
        }

        private void EmergencyContacts_ItemSelected(object sender, ItemTappedEventArgs e)
        {
            var contact = e.Item as Contact;
            if(contact.name.Length > 0)
            {
                contactListSource[allContacts.get(contact.name)].isEmergencyContact = false;
                contactListSource[allContacts.get(contact.name)].isVisible = true;
            }
            else
            {
                customNumbers.Remove(contact.displayNumber);
            }
            refreshLists();
            dirtyFlag = true;
        }

        private async void ContactList_ItemSelected(object sender, ItemTappedEventArgs e)
        {
            var contact = e.Item as Contact;
            int index = allContacts.get(contact.name);
            if(contact.numbers.Length > 1)
            {
                var popupManager = Rg.Plugins.Popup.Services.PopupNavigation.Instance;
                string num = "";
                object conditionalVariable = new object();
                popupManager.Popped += (object s, Rg.Plugins.Popup.Events.PopupNavigationEventArgs arg) =>
                {
                    var poppedPage = arg.Page as NumSelectPopup;
                    num = poppedPage.getSelectedNumbers();
                    if (num != null && num.Length >= 7)
                    {
                        contactListSource[index].displayNumber = num;
                        contactListSource[index].isEmergencyContact = true;
                        contactListSource[index].isVisible = false;
                        refreshLists();
                        dirtyFlag = true;
                    }
                };
                await popupManager.PushAsync(new NumSelectPopup(contact.numbers), true);
            }
            else
            {
                contactListSource[index].displayNumber = contact.displayNumber;
                contactListSource[index].isEmergencyContact = true;
                contactListSource[index].isVisible = false;
                refreshLists();
                dirtyFlag = true;
            }           

        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            var list = allContacts.getWords(e.NewTextValue);
            for (int i = 0; i < contactListSource.Count; ++i)
                contactListSource[i].isVisible = false;
            foreach (var pair in list)
            {
                if (!contactListSource[pair.Value].isEmergencyContact)
                    contactListSource[pair.Value].isVisible = true;
            }
            refreshLists();
        }
        private async void populateLists()
        {
            contactListSource = new ObservableCollection<Contact>();
            var list = await DependencyService.Get<IContactService>().asyncGetContacts();
            var ems = Serialize.deserializeEmergencyContacts(Store.load(StoreDataType.EmergencyList));
            customNumbers.Clear();
            var customs = Serialize.deserializeCustomNumbers(Store.load(StoreDataType.CustomNumbers));
            var emergencyContacts = ems;
            if (customs != null)
            {
                foreach(var n in customs)
                {
                    if (n.Length >= 7)
                    {
                        emergencyContacts.Add(new Contact()
                        {
                            displayNumber = n,
                            numbers = new[] { n },
                            name = ""
                        });
                        customNumbers.Add(n);
                    }
                }
            }
            if (ems != null)
            {
                ems.Sort(new Comparison<Contact>((Contact a, Contact b) =>
                {
                    return a.name.CompareTo(b.name);
                }));               
                for (int i = 0, j = 0; i < list.Count; ++i)
                {
                    foreach (var num in list[i].numbers)
                        list[i].displayNumber += num + " ";
                    if (j < ems.Count && list[i].name.Equals(ems[j].name))
                    {
                        list[i].isVisible = false;
                        list[i].isEmergencyContact = true;
                        list[i].displayNumber = ems[j].displayNumber;
                        ++j;
                    }
                    else list[i].isVisible = true;
                    contactListSource.Add(list[i]);
                    allContacts.add(list[i].name, i);
                }
            }
            //           contactList.ItemsSource = contactListSource;
            //           emergencyContacts.ItemsSource = contactListSource;
            refreshLists();
            
        }
        private void refreshLists()
        {
            ObservableCollection<Contact> ct = new ObservableCollection<Contact>();
            ObservableCollection<Contact> em = new ObservableCollection<Contact>();           
            var list = allContacts.getWords();
            foreach(var item in list)
            {
                if (contactListSource[item.Value].isEmergencyContact)
                    em.Add(contactListSource[item.Value]);
                else if(contactListSource[item.Value].isVisible)
                    ct.Add(contactListSource[item.Value]);
            }
            foreach(var n in customNumbers)
            {
                em.Add(new Contact()
                {
                    displayNumber = n, numbers = new[] {n},
                    name = "", isEmergencyContact = true, isVisible = false
                });
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                emergencyContacts.ItemsSource = null;
                emergencyContacts.ItemsSource = em;
                contactList.ItemsSource = null;
                contactList.ItemsSource = ct;
            });           
        }

        private void save()
        {
            List<Contact> emergencies = new List<Contact>();
            foreach(var ct in contactListSource)
            {
                if (ct.isEmergencyContact) emergencies.Add(ct);
            }
            Store.save(StoreDataType.EmergencyList, Serialize.serializeEmergencyContacts(emergencies));
            foreach (var num in customNumbers)
            {
                emergencies.Add(new Contact()
                {
                    displayNumber = num,
                    numbers = new[] { num },
                    name = ""
                });
            }           
            Store.save(StoreDataType.CustomNumbers, Serialize.serializeCustomNumbers(customNumbers));
            Mediator.invoke(this, new ContactChangedEventArgs() { contacts = emergencies });
            dirtyFlag = false;
        }

        private async void CustomContactBtn_Clicked(object sender, EventArgs e)
        {
            string num = "";
            bool loop = false;
            do
            {
                num = await DisplayPromptAsync(AppResources.AddNumTitle, (loop ? AppResources.ValidNum + " " : "") + AppResources.AddNumDesc, AppResources.Add, AppResources.Cancel, null, -1, Keyboard.Telephone);
            } while (num != null && num.Length < 7 && (loop = true));
            if(num != null)
            {
                customNumbers.Add(num);
                refreshLists();
                dirtyFlag = true;
            }
        }
    }
}
