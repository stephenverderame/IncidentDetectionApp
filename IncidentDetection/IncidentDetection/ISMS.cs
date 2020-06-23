using System;
using System.Collections.Generic;
using System.Text;

namespace IncidentDetection
{
    public delegate void HangupListener();
    public delegate void SmsReceivedEvent(string[] senderAddresses);
    public interface ISMS
    {
        //*Implementor should check if numbers are valid
        void sendSms(string[] numbers, string msg);
    }
    public interface IPhoneCall
    {
        void makeCall(string number);
        void enableSpeaker();
    }
    public interface IPhoneListener
    {        
        void startListenSms(SmsReceivedEvent onSms);
        void stopListenSms();
 //       void startListenCalls(HangupListener onHangup);
 //       void stopListenCalls();
    }
}
