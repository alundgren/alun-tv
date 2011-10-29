//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web;
//using SignalR;

//namespace WebUi.Domain.Events
//{
//    public class EventConnection : PersistentConnection
//    {
//        private readonly Dictionary<string, Func<string, string, Task>> _eventHandlers;
        
//        public EventConnection()
//        {
//            _eventHandlers = new Dictionary<string, Func<string, string, Task>>();

//            _eventHandlers.Add("EpisodeWatched", 
//                (clientId, data) =>
//                    {
//                        return Connection.Send("hi");
//                    });
//        }
        
//        protected override Task OnReceivedAsync(string clientId, string data)
//        {
//            return Connection.Broadcast(data);
//            //var i = (data??"").IndexOf("/");
//            //if (i < 0 || i == (data.Length - 1))
//            //    return Connection.Send("error");

//            //var eventName = data.Substring(0, i);

//            //if (_eventHandlers.ContainsKey(eventName))
//            //    return _eventHandlers[eventName](clientId, data.Substring(i+1));

//            //return Connection.Send("error");
//        }
//    }
//}