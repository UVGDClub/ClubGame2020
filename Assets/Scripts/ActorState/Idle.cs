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
            //enemy.StartCoroutine(IdleGrowl(2.0f));
        }
        private IEnumerator IdleGrowl(float waitTime)
        {
            float growelTime; 
            while (true)
            {
                growelTime = Random.Range(0, 4);
                yield return new WaitForSeconds(growelTime);
                //audio[Growel].Play();
            }


        }
        private bool searchForPlayer(Enemy enemy)
        {
            Vector3 dist;
            RaycastHit hit;
            //transform.position = new Vector3( x, player.transform.position.y, z);
            Vector3 origin = new Vector3(enemy.transform.position.x, 1f, enemy.transform.position.z);
            //Vector3 direction = new Vector3(10, 0, 10);
            Ray ray = new Ray(origin, enemy.transform.forward);
            Physics.Raycast(ray, out hit);
            if (hit.transform)
            {
                Debug.Log("ray cast: " + hit.transform.position);
            }
            //transform.position = new Vector3(x, player.transform.position.y, z);
            if (hit.transform && hit.transform.CompareTag("Player"))
            {

                dist = enemy.transform.position - hit.transform.position;
                if(dist.magnitude < 20)
                {
                    return true;
                }
            }
            return false;
        }
        public override IEnumerator Execute(Enemy enemy)
        {
            bool playerSpotted = false;
            while (enemy.curState.GetInstanceID() == GetInstanceID())
            {
                Debug.Log("Searching for player");
                //look for player
                playerSpotted = searchForPlayer(enemy);
                //if we find them...
                if(playerSpotted)
                {
                    enemy.stateSchema.approach.OnEnter(enemy);
                    yield break; //end this coroutine
                }
                else
                {
                    Debug.Log("Idle, rotating");
                    enemy.transform.Rotate(0, 1f, 0);
                }
                yield return null;
            }
        }

        public override void OnExit(Enemy enemy)
        {
            //not sure if this will be needed in this state
            foreach(StateTransitionCondition c in transitionCondition)
            {
                if (c.condition(enemy))
                    c.state.OnEnter(enemy);
            }
        }
    }
}