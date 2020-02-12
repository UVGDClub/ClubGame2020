using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class Retreat : BaseState
    {
        public override void OnEnter(Enemy enemy)
        {
            base.OnEnter(enemy);
        }

        public override IEnumerator Execute(Enemy enemy)
        {
            return base.Execute(enemy);
        }

        public override void OnExit(Enemy enemy)
        {
            base.OnExit(enemy);
        }
    }
}
