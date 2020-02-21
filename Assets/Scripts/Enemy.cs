using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine;

public class Enemy : Actor
{
    public StateSchema stateSchema;
    public BaseState curState;

    private void Awake()
    {
        Init();
    }

    //todo: call from game master
    public void Init()
    {
        stateSchema.idle.OnEnter(this);
    }
}