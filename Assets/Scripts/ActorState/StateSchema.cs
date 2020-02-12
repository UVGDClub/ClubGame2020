using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine;

[CreateAssetMenu(fileName = "new State Schema", menuName = "State Machine/State Schema")]
public class StateSchema : ScriptableObject
{
    public Idle idle;
    public Attack attack;
    public Retreat retreat;
}