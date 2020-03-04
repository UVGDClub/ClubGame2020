using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class PlayerSpottedTransitionCondition : StateTransitionCondition
    {
        public override bool condition(Enemy enemy)
        {
            if(enemy.target != null)
                return true;
            return false;
        }
    }
}