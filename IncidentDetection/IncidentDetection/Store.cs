using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace IncidentDetection
{
    public class StoreDataType
    {
        public string dataType { get; private set; }
        private StoreDataType(string type)
        {
            dataType = type;
        }
        override public string ToString()
        {
            return dataType;
        }

        public static StoreDataType EmergencyList { get { return new StoreDataType("emg_cts"); } }
        public static StoreDataType CustomNumbers { get { return new StoreDataType("cst_num"); } }
        public static StoreDataType NullData { get { return new StoreDataType("nll_dta"); } } 
        public static StoreDataType Settings { get { return new StoreDataType("set_dta"); } }
    }
    public class Store
    {
        public static void save(StoreDataType key, string data)
        {
            Preferences.Set(key.dataType, data);
        }
        public static string load(StoreDataType key)
        {
            return Preferences.Get(key.ToString(), StoreDataType.NullData.ToString());
        }

    }
    public class Serialize
    {
        public static string serializeEmergencyContacts(IList<Contact> emergencies)
        {
            string str = StoreDataType.EmergencyList.ToString();
            foreach(var ct in emergencies)
            {
                str += "[" + ct.name + ":" + ct.displayNumber + "]";
            }
            return str;
        }
        public static List<Contact> deserializeEmergencyContacts(string serializedData)
        {
            if (serializedData == StoreDataType.NullData.ToString()) return null;
            var emergencies = new List<Contact>();
            var t = serializedData.Substring(0, 7);
            var tt = StoreDataType.EmergencyList.ToString();
            if(serializedData.Substring(0, 7).Equals(StoreDataType.EmergencyList.ToString()))
            {
                int start = -1;
                for(int i = 7; i < serializedData.Length; ++i)
                {
                    if (serializedData[i] == '[') start = i;
                    else if(serializedData[i] == ']' && start > 1)
                    {
                        string contact = serializedData.Substring(start + 1, i - start - 1);
                        string name = contact.Substring(0, contact.IndexOf(':'));
                        string[] nums = contact.Substring(contact.IndexOf(':') + 1).Split(',');
                        emergencies.Add(new Contact()
                        {
                            name = name,
                            numbers = nums,
                            displayNumber = contact.Substring(contact.IndexOf(':') + 1)
                        });
                    }
                }
            }
            return emergencies;
        }
        public static string serializeCustomNumbers(IList<string> numbers)
        {
            string n = "";
            foreach (var c in numbers)
                n += c + "|";
            return n;
        }
        public static string[] deserializeCustomNumbers(string nums)
        {
            return nums == StoreDataType.NullData.dataType ? null : nums.Split('|');
        }
        public static Dictionary<string, double> deserializeSettings(string settings)
        {
            if (settings == StoreDataType.NullData.ToString() || settings.Length < 7) return null;
            Dictionary<string, double> settingsData = SettingsPage.DEFAULT_SETTINGS;
            if(settings.Substring(0, 7).Equals(StoreDataType.Settings.ToString()))
            {
                string vals = settings.Substring(7);
                int start = -1;
                for(int i = 0; i < vals.Length; ++i)
                {
                    if (vals[i] == '[') start = i;
                    else if(vals[i] == ']' && start != -1)
                    {
                        string[] data = vals.Substring(start + 1, i - start - 1).Split(':');
                        if(data.Length == 2)
                        {
                            settingsData[data[0]] = Convert.ToDouble(data[1]);
                        }
                    }
                }

            }
            else
            {
                return null;
            }
            return settingsData;
        }

        public static string serializeSettings(KeyValuePair<string, double>[] settings)
        {
            string serialize = StoreDataType.Settings.ToString();
            foreach(var pair in settings)
            {
                serialize += '[' + pair.Key + ':' + Convert.ToString(pair.Value) + ']';
            }
            return serialize;
        }
    }
}
