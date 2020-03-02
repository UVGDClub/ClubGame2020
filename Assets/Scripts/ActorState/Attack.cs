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
            Debug.Log("Attack!");
            enemy.StartCoroutine(Execute(enemy));
        }

        public override IEnumerator Execute(Enemy enemy)
        {
            yield return new WaitForSeconds(0.8f);
            OnExit(enemy);
            /*while (enemy.curState.GetInstanceID() == GetInstanceID())
            {
               // Debug.Log(enemy.anim.GetInteger("Anim_isSwinging"));
				//current tranition between states may cause crashes
				/*
				if(enemy.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
				{
					enemy.anim.SetInteger("Anim_isRunning", 0);
					enemy.anim.SetInteger("Anim_isSwinging", 0);
					enemy.stateSchema.idle.OnEnter(enemy);
					yield break;
				}
				
                yield return null;
            }*/
        }

        public override void OnExit(Enemy enemy)
        {
            //not sure if this will be needed in this state
            enemy.stateSchema.idle.OnEnter(enemy);
        }
    }
}
