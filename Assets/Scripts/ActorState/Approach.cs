using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class Approach : BaseState
    {
        public override void OnEnter(Enemy enemy)
        {
            enemy.curState = this;
            enemy.StartCoroutine(Execute(enemy));
            enemy.anim.SetInteger("Anim_isRunning", 1);
        }
        private Vector3 searchForPlayer(Enemy enemy,float callTime)
        {
            RaycastHit hit;
            float x;
            Ray ray;
            float z;
            do
            {
                x = Random.Range(-50, 50);
                z = Random.Range(-50, 50);
                //transform.position = new Vector3( x, player.transform.position.y, z);
                Vector3 location = new Vector3(x, 1f, z);
                Vector3 direction = enemy.transform.forward;
                ray = new Ray(location, direction);
                Physics.Raycast(ray, out hit);
                if (hit.transform && hit.transform.CompareTag("Player"))
                    break;
            } while (Time.time - callTime < 0.5f);

            return hit.transform.position;
        }
        public override IEnumerator Execute(Enemy enemy)
        {
            Vector3 playerLocation;
            Vector3 dist;
            while (enemy.curState.GetInstanceID() == GetInstanceID())
            {
                playerLocation = searchForPlayer(enemy,Time.time);
                enemy.agent.SetDestination(playerLocation);
                dist = playerLocation - enemy.transform.position;
                if(dist.magnitude < 2.0f)
                {
					Debug.Log("Attack!");
                    enemy.stateSchema.attack.OnEnter(enemy);
                    yield break; //end this coroutine
                }
                yield return null;
            }
        }
        public override void OnExit(Enemy enemy)
        {
			Debug.Log("End Approach");
            enemy.anim.SetInteger("Anim_isRunning", 0);
        }
    }
}