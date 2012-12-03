using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlazeGames.Web.Core
{
    public class Events
    {
        private Dictionary<string, Func<bool>> EventList = new Dictionary<string, Func<bool>>();

        public void Register(string EventName, Func<bool> CallBackMethod)
        {
            EventList.Add(EventName, CallBackMethod);
        }

        public void UnRegister(string EventName)
        {
            EventList.Remove(EventName);
        }

        public void Call(string EventName)
        {
            if(EventList.ContainsKey(EventName))
            EventList[EventName]();
        }
    }
}