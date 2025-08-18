using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class AttackState<T> : State<T>
{
    ZombieView _view;
    NavMeshAgent _agent;
    Action _changetarget;
    public AttackState(ZombieView view, NavMeshAgent agent, Action changetarget)
    {
        _view = view;
        _agent = agent;
        _changetarget = changetarget;
    }
    public override void Enter()
    {
        base.Enter();
        _view.Attack();
        _changetarget.Invoke();
        _agent.isStopped = true;
    }

    public override void Exit()
    {
        base.Exit();
        _agent.isStopped = false;
    }
}
