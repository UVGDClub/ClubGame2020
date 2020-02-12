using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class Idle : BaseState
    {
        public override void OnEnter(Enemy enemy)
        {
            enemy.curState = this;
            enemy.StartCoroutine(Execute(enemy));
        }

        public override IEnumerator Execute(Enemy enemy)
        {
            bool playerSpotted = false;
            while (enemy.curState.GetInstanceID() == GetInstanceID())
            {
                //look for player

                //if we find them...
                if(playerSpotted)
                {
                    enemy.stateSchema.attack.OnEnter(enemy);
                    yield break; //end this coroutine
                }
                yield return null;
            }
        }

        public override void OnExit(Enemy enemy)
        {
            //not sure if this will be needed in this state
        }
    }
}