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
            if(enemy.gameController.numPlayers == 0)
            {
                return false;
            }
            float minDist = int.MaxValue;
            Vector2 curDist;
            foreach(GameObject g in enemy.gameController.activePlayers)
            {
                if (g != null)
                {
                    curDist = g.transform.position - enemy.transform.position;
                    if (curDist.magnitude < minDist)
                    {
                        enemy.target = g.transform;
                        minDist = curDist.magnitude;
                    }
                }
            }
            return true;
        }
        public override IEnumerator Execute(Enemy enemy)
        {
            bool playerSpotted = false;
            while (enemy.curState.GetInstanceID() == GetInstanceID())
            {
                //look for player
                playerSpotted = searchForPlayer(enemy);
                //if we find them...
                if(playerSpotted)
                {
                    Debug.Log("Player found!");
                    OnExit(enemy);
                    //enemy.stateSchema.approach.OnEnter(enemy);
                    yield break; //end this coroutine
                }
                else
                {
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