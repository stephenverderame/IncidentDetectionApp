using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IncidentDetection
{
    public class Contact : INotifyPropertyChanged
    {
        public string name { get; set; }
        public string[] numbers { get; set; }
        //string to display in contact list. Comma separated list of numbers. The actual numbers messaged 
        public string displayNumber { get; set; }
        public bool isEmergencyContact { get; set; }
        public bool isVisible { get; set; }
        public Contact() { }
        public Contact(Contact other)
        {
            name = other.name;
            numbers = other.numbers;
            displayNumber = other.displayNumber;
            isEmergencyContact = other.isEmergencyContact;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    //Encapsulates the contact that was just read
    public class ContactLoadEventArgs
    {
        public Contact contact { get; private set; }
        public ContactLoadEventArgs(Contact contact)
        {
            this.contact = contact;
        }
    }
    public interface IContactService
    {
        //Function called each time a contact is read from the phone
        event EventHandler<ContactLoadEventArgs> onContactLoaded;
        Task<IList<Contact>> asyncGetContacts(CancellationToken? tk = null);
    }
}
