using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class Attack : BaseState
    {
        public override void OnEnter(Enemy enemy)
        {
			enemy.curState = this;
			enemy.anim.SetInteger("Anim_isSwinging", 1);
            enemy.StartCoroutine(Execute(enemy));
        }

        public override IEnumerator Execute(Enemy enemy)
        {
            while (enemy.curState.GetInstanceID() == GetInstanceID())
            {
				
				//current tranition between states may cause crashes
				/*
				if(enemy.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
				{
					enemy.anim.SetInteger("Anim_isRunning", 0);
					enemy.anim.SetInteger("Anim_isSwinging", 0);
					enemy.stateSchema.idle.OnEnter(enemy);
					yield break;
				}
				*/
                yield return null;
            }
        }

        public override void OnExit(Enemy enemy)
        {
            
        }
    }
}
