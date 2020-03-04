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

            //transform.position = new Vector3( x, player.transform.position.y, z);
            Vector3 location = new Vector3(enemy.transform.position.x, 1f, enemy.transform.position.z);
            Vector3 direction = enemy.transform.forward;
            ray = new Ray(location, direction);
            Physics.Raycast(ray, out hit,maxDistance:2);
            if (hit.transform && hit.transform.CompareTag("Player"))
                return hit.transform.position;
            return enemy.transform.position;

        }
        public override IEnumerator Execute(Enemy enemy)
        {
            Vector3 dist;
            while (enemy.curState.GetInstanceID() == GetInstanceID())
            {
                enemy.agent.SetDestination(enemy.target.transform.position);
                dist = enemy.target.transform.position - enemy.transform.position;
                if (dist.magnitude < 2.0f)
                {

                    OnExit(enemy);


                    yield break; //end this coroutine
                }
                
                yield return null;
            }
        }
        public override void OnExit(Enemy enemy)
        {
            Debug.Log("End Approach");
            enemy.anim.SetInteger("Anim_isSwinging", 1);
            enemy.agent.SetDestination(enemy.transform.position);

            enemy.stateSchema.attack.OnEnter(enemy);
        }
    }
}