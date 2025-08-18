using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IdleState<T> : State<T>
{
    NavMeshAgent _agent;
    Action _action;
    public IdleState(NavMeshAgent agent, Action coroutine) 
    {
        _agent = agent;
        _action = coroutine;
    }
    public override void Enter()
    {
        base.Enter();
        _action.Invoke();
        Debug.Log("i'm idle");
    }
  
}
