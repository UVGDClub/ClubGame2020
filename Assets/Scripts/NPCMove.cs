using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCMove : MonoBehaviour
{
    [SerializeField]
    Transform dest;

    NavMeshAgent nAgent;
    // Start is called before the first frame update
    void Start()
    {
        nAgent = this.GetComponent<NavMeshAgent>();
        if(nAgent == null)
        {
            Debug.LogError("The nav mesh agent component is not attached to " + gameObject.name);
        }
        else
        {
            SetDestination();
        }
    }

    private void SetDestination()
    {
        if(dest != null)
        {
            Vector3 targetVector = dest.transform.position;
            nAgent.SetDestination(targetVector);
        }
    }
}
