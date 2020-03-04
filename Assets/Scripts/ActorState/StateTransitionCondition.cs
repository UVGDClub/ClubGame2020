using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(fileName = "new TransitionCondition", menuName = "State Transition/base condition")]
    public class StateTransitionCondition : ScriptableObject
    {
        public BaseState state;
        public StateTransitionCondition[] subConditions;
        public virtual bool condition(Enemy enemy)
        {
            foreach (StateTransitionCondition c in subConditions)
                if (c.condition(enemy) == false)
                    return false;

            return true;
        }
    }
}