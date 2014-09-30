using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActiveStateMachine
{
    class ActiveStateMachine
    {
        public Dictionary<String,State> StateList { get; private set; }
        public BlockingCollection<string> TriggerQueue { get; private set; }
        public State CurrentState { get; private set; }
        public State PreviousState { get; private set; } //for debugging purposes mostly
        public EngineState StateMachineEngine { get; private set; } //for engine

        public event EventHandler<StateMachineEventArgs> StateMachineEvent;


        //private
        private Task _queueWorkerTask;  //queue worker task where transitions are ....
        private readonly State _initialState;
        private ManualResetEvent _resumer;  //to pause the state machine
        private CancellationTokenSource _tokenSource; //to shut down the system gracefully

        public ActiveStateMachine(Dictionary<String, State> stateList,int queueCapacity)
        {
            //queueCapacity to prevent denial of service
            StateList = stateList;
            _initialState = new State("InitialState", null, null, null);
            TriggerQueue = new BlockingCollection<string>(queueCapacity);
            InitStateMAchine();
            RaiseStateMachineSystemEvent("StateMachine: Initialized", "System ready to start");
            StateMachineEngine = EngineState.Initialized;
        }

        private void InitStateMAchine()
        {
            PreviousState = _initialState;
            foreach (var state in StateList)
            {
                if (state.Value.IsDefaultState)
                {
                    CurrentState = state.Value;
                    RaiseStateMachineSystemCommand("OnInit","StateMachineInitialized");
                }
            }
            _resumer = new ManualResetEvent(true);
        }

        #region event Infrastructure
        
        private void RaiseStateMachineSystemEvent(string eventName, string eventInfo)
        {
            //not as strict as command
            if (StateMachineEvent != null)
                StateMachineEvent(this, new StateMachineEventArgs(eventName, eventInfo, StateMachineEventType.System, "State machine"));
        }

        private void RaiseStateMachineSystemCommand(string eventName, string eventInfo)
        {
            //if subscriber avail, raises event
            if (StateMachineEvent != null)
                StateMachineEvent(this, new StateMachineEventArgs(eventName, eventInfo, StateMachineEventType.Command, "State machine"));
        }

        #endregion

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();
            _queueWorkerTask = Task.Factory.StartNew(QueueWorkerMethod, _tokenSource, TaskCreationOptions.LongRunning);
            StateMachineEngine = EngineState.Running;
            RaiseStateMachineSystemEvent("StateMachine: Started", "System running");
        }
        public void Pause()
        {
            StateMachineEngine = EngineState.Paused;
            _resumer.Reset();
            RaiseStateMachineSystemEvent("StateMachine: Paused", "System waiting");
        }
        public void Resume()
        {
            _resumer.Set();
            StateMachineEngine = EngineState.Running;

            RaiseStateMachineSystemEvent("StateMachine: Resumed", "System running");
        }
        public void Stop()
        {
            _tokenSource.Cancel();
            _queueWorkerTask.Wait(); //to end gracefully
            _queueWorkerTask.Dispose();
            StateMachineEngine = EngineState.Stopped;

            RaiseStateMachineSystemEvent("StateMachine: Stopped", "System execution stopped");
        }

        private void QueueWorkerMethod(object dummy)
        {
            _resumer.WaitOne();
            try
            {
                foreach (var trigger in TriggerQueue.GetConsumingEnumerable()) //not using take method, using this one to get avail triggers in the queue
                {
                    if (_tokenSource.IsCancellationRequested)
                    {
                    }
                    foreach (var transition in CurrentState.StateTransitionList.Where(transition => trigger==transition.Value.Trigger))
                    {
                        ExecuteTransition(transition.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseStateMachineSystemEvent("State machine: Queueworker", "Processing canceled! Exception: " + ex.ToString());
                Start();
            }
        }

        //real work done here
        protected virtual void ExecuteTransition(Transition transition)
        {
            // Default checking, if this is a valid transaction.
            if (CurrentState.StateName != transition.SourceStateName)
            {
                String message =
                    String.Format("Transition has wrong source state {0}, when system is in {1}",
                        transition.SourceStateName, CurrentState.StateName);
                RaiseStateMachineSystemEvent("State machine: Default guard execute transition.", message);
                return;
            }
            if (!StateList.ContainsKey(transition.TargetStateName))
            {
                String message =
                        String.Format("Transition has wrong target state {0}, when system is in {1}. State not in global state list",
                            transition.SourceStateName, CurrentState.StateName);
                RaiseStateMachineSystemEvent("State machine: Default guard execute transition.", message);
                return;
            }

            // Run all exit actions of the old state
            CurrentState.ExitActions.ForEach(a => a.Execute());

            // Run all guards of the transition
            transition.GuardList.ForEach(g => g.Execute());
            string info = transition.GuardList.Count + " guard actions executed!";
            RaiseStateMachineSystemEvent("State machine: ExecuteTransition", info);

            // Run all actions of the transition
            transition.TransitionActionList.ForEach(t => t.Execute());


            //////////////////
            // State change
            //////////////////
            info = transition.TransitionActionList.Count + " transition actions executed!";
            RaiseStateMachineSystemEvent("State machine: Begin state change!", info);


            // First resolve the target state with the help of its name
            var targetState = GetStatefromStateList(transition.TargetStateName);

            // Transition successful - Change state
            PreviousState = CurrentState;
            CurrentState = targetState;

            // Run all entry actions of new state
            foreach (var entryAction in CurrentState.EntryActions)
            {
                entryAction.Execute();
            }

            RaiseStateMachineSystemEvent("State machine: State change completed successfully!", "Previous state: "
                + PreviousState.StateName + " - New state = " + CurrentState.StateName);
        }
        private State GetStatefromStateList(string targetStateName)
        {
            return StateList[targetStateName];
        }

        private void EnterTrigger(string newTrigger)
        {
            try
            {
                TriggerQueue.Add(newTrigger);
            }
            catch (Exception e)
            {
                RaiseStateMachineSystemEvent("ActiveStateMachine - Error entering Trigger", newTrigger + " - " + e.ToString());
            }
            RaiseStateMachineSystemEvent("ActiveStateMachine - Trigger entered", newTrigger);
        }

        public void InternalNotificationHandler(object sender, StateMachineEventArgs intArgs)
        {
            EnterTrigger(intArgs.EventName);
            //EnterTrigger is private and called eith event handler here
        }
    }

    public enum EngineState
	{
        Running,
        Stopped, 
        Paused, 
        Initialized
	}
}
