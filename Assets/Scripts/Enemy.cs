using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine;
using UnityEngine.AI;

public class Enemy : Actor
{
    [SerializeField]
    public StateSchema stateSchema;
    [SerializeField]
    public BaseState curState;
    [SerializeField]
    public NavMeshAgent agent;
    [SerializeField]
    public Animator anim;
    [SerializeField]
    public GameController gameController;
    
    public Transform target;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        if(gameController==null)
        {
            gameController = FindObjectOfType<GameController>();
        }
        target = null;
        Init();
    }

    //todo: call from game master
    public void Init()
    {
        stateSchema.idle.OnEnter(this);
    }
    public IEnumerator OnDeath()
    {
        anim.SetInteger("Anim_isDead", 1);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);

    }
}