using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveStateMachine
{
    public class Transition
    {
        public string Name { get; private set; }
        public string SourceStateName { get; private set; } //need to know 2 state names
        public string TargetStateName { get; private set; }
        //collection of state machine actions
        public List<StateMachineAction> GuardList { get; private set; }  //list of checks that need to be checked before state change
        public List<StateMachineAction> TransitionActionList { get; private set; } // real work here
        public string Trigger { get; private set; } //used by state machine 

        public Transition(string name, string sourceStateName, string targetStateName, List<StateMachineAction> guardList, List<StateMachineAction> transitionActionList, string trigger)
        {
            Name = name;
            SourceStateName = sourceStateName;
            TargetStateName = targetStateName;
            GuardList = guardList;
            TransitionActionList = transitionActionList;
            Trigger = trigger;
        }
    }
}
