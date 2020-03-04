using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StateMachine
{
    public abstract class BaseState : ScriptableObject
    {
        public StateTransitionCondition[] transitionCondition;

        public virtual void OnEnter(Enemy enemy)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerator Execute(Enemy enemy)
        {
            throw new NotImplementedException();
        }

        public virtual void OnExit(Enemy enemy)
        {
            throw new NotImplementedException();
        }
    }
}