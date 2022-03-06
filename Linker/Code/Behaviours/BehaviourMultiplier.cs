using Linker.Nodes;
using System;

namespace Linker.Code.Behaviours
{
    public class BehaviourMultiplier : Behaviour
    {

        public override void Count(object input)
        {
            if (input == null)
                return;

            double tempVal;
            if (double.TryParse(input.ToString(), out tempVal))
                Value = Multiplier * tempVal;          
        }
    }
}
