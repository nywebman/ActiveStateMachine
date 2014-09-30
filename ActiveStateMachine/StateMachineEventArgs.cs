using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveStateMachine
{
    class StateMachineEventArgs
    {
        public string EventName { get; private set; }
        public string EventInfo { get; private set; } //more details about event
        public DateTime TimeStamp { get; private set; } //for order of events
        public string Source { get; private set; }
        public string Target { get; private set; }
        public StateMachineEventType EventType { get; set; }

        public StateMachineEventArgs(string eventName, string eventInfo, StateMachineEventType eventType, string source, string target="All")
        {
            EventName = eventName;
            EventInfo = eventInfo;
            EventType = eventType;
            Source = source;
            Target = target;
            TimeStamp = DateTime.Now;
        }
    }
}
