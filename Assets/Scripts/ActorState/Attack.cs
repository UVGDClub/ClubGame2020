using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class Attack : BaseState
    {
        public override void OnEnter(Enemy enemy)
        {
            
        }

        public override IEnumerator Execute(Enemy enemy)
        {
            while (enemy.curState.GetInstanceID() == GetInstanceID())
            {
                yield return null;
            }
        }

        public override void OnExit(Enemy enemy)
        {
            
        }
    }
}
