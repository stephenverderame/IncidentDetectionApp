using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;

[assembly: Xamarin.Forms.Dependency(typeof(IncidentDetection.Droid.ContactService))]
namespace IncidentDetection.Droid
{
    public class ContactService : IContactService
    {
        public const int PERMISSION_CONTACTS_REQUEST = 1239;

        public event EventHandler<ContactLoadEventArgs> onContactLoaded;

        public async Task<IList<Contact>> asyncGetContacts(CancellationToken? tk = null)
        {
            if(ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.ReadContacts) != (int)Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(MainActivity.activity, new string[] { Manifest.Permission.ReadContacts }, PERMISSION_CONTACTS_REQUEST);
            }
            return await loadContactsAsync();

        }
        private async Task<IList<Contact>> loadContactsAsync()
        {
            IList<Contact> contacts = new List<Contact>();
            var uri = ContactsContract.Contacts.ContentUri;
            await Task.Run(() =>
            {
                var cursor = Application.Context.ApplicationContext.ContentResolver.Query(uri, new string[]
                {
                    ContactsContract.Contacts.InterfaceConsts.Id,
                    ContactsContract.Contacts.InterfaceConsts.DisplayName
                }, null, null, $"{ContactsContract.Contacts.InterfaceConsts.DisplayName} ASC"); //queries contact database and sorts by display name
                if(cursor.Count > 0)
                {
                    while(cursor.MoveToNext())
                    {
                        var contactId = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id));
                        var numCursor = Application.Context.ApplicationContext.ContentResolver.Query(ContactsContract.CommonDataKinds.Phone.ContentUri, null,
                            ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + " = ?", new[] { contactId }, null);
                        var nums = new List<string>();
                        while(numCursor.MoveToNext())
                        {
                            nums.Add(numCursor.GetString(numCursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number))); 

                        }
                        numCursor.Close();
                        var newContact = new Contact()
                        {
                            name = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName)),
                            numbers = nums.ToArray()
                        };
                        contacts.Add(newContact);
                        onContactLoaded?.Invoke(this, new ContactLoadEventArgs(newContact)); //?. is a null conditional, basically if the first is null, it returns null otherwise executes the member function
                    }
                }
            });
            return contacts;
        }
    }
}