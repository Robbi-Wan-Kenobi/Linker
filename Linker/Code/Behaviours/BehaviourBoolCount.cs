using Linker.Nodes;
using System;
using System.ComponentModel;

namespace Linker.Code.Behaviours
{
    public class BehaviourBoolCount : Behaviour
    { 
        public override void Count(object input)
        {
            string tempVal = input as string;

            if(tempVal != null  && tempVal.Equals(triggerValue, StringComparison.OrdinalIgnoreCase))
                Value += Multiplier;            
        }



        private string triggerValue = string.Empty;

        /// <summary>
        /// The value that triggers a possitive count
        /// </summary>
        public string TriggerValue 
        {
            get { return triggerValue; }
            set 
            { 
                triggerValue = value;
                base.RaisePropertyChanged(nameof(TriggerValue));
            }
        }

    }
}
