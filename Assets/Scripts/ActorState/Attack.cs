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
            yield return null;
        }

        public override void OnExit(Enemy enemy)
        {
            
        }
    }
}
