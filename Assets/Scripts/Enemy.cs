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
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        Init();
    }

    //todo: call from game master
    public void Init()
    {
        stateSchema.idle.OnEnter(this);
    }
}