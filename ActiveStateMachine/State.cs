using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveStateMachine
{
    public class State
    {
        public string StateName { get; private set; }
        public Dictionary<string, Transition> StateTransitionList { get; private set; }
        public List<StateMachineAction> EntryActions { get; private set; }
        public List<StateMachineAction> ExitActions { get; private set; }
        public bool IsDefaultState { get; private set; } //need to tell state machine which is default on init

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="transitionList"></param>
        /// <param name="entryActions"></param>
        /// <param name="exitActions"></param>
        /// <param name="defaultState"></param>
        public State(string stateName, Dictionary<string, Transition> transitionList, List<StateMachineAction> entryActions, List<StateMachineAction> exitActions, bool defaultState=false)
        {
            StateName = stateName;
            StateTransitionList = transitionList;
            EntryActions = entryActions;
            ExitActions = exitActions;
            IsDefaultState = defaultState;
        }
    }
}
