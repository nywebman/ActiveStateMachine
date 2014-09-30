using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveStateMachine
{
    public class StateMachineAction
    {
        //public
        public string Name { get; private set; } //private bcse only want set through constructor

        //private
        //delegate pointing to implemetation of method. this one accepts no parameters
        private System.Action _method;


        public StateMachineAction(string name, Action method)
        {
            Name = name;
            _method = method;
        }

        public void Execute()
        {
            _method.Invoke();
        }
    }
}
