using System;
using System.Collections.Generic;
using System.Text;

namespace IncidentDetection
{
    public class ContactChangedEventArgs
    {
        public List<Contact> contacts { get; set; }
    }
    public class SettingsChangedEventArgs
    {
        public Dictionary<string, double> Settings { get; set; }
    }
    public delegate void ContactsChanged(object sender, ContactChangedEventArgs args);
    public delegate void SettingsChanged(object sender, SettingsChangedEventArgs args);
    public class Mediator
    {
        private static List<ContactsChanged> contactDelegates = new List<ContactsChanged>();
        private static List<SettingsChanged> settingsDelegates = new List<SettingsChanged>();
        private static ContactChangedEventArgs lastContactsArg = null;
        private static SettingsChangedEventArgs lastSettingsArg = null;
        public static void subscribe(ContactsChanged del, bool handleMissedCalls = false)
        {
            if (handleMissedCalls && lastContactsArg != null)
                del(null, lastContactsArg);
            contactDelegates.Add(del);
        }
        public static void subscribe(SettingsChanged del, bool handleMissedCall = false)
        {
            if (handleMissedCall && lastSettingsArg != null)
                del(null, lastSettingsArg);
            settingsDelegates.Add(del);
        }
        public static void invoke(object sender, ContactChangedEventArgs arg)
        {
            lastContactsArg = arg;
            foreach (var del in contactDelegates)
                del.Invoke(sender, arg);
        }
        public static void invoke(object sender, SettingsChangedEventArgs e)
        {
            lastSettingsArg = e;
            foreach (var del in settingsDelegates)
                del.Invoke(sender, e);
        }
    }
}
