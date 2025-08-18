using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM<T>
{
    IState<T> _currState;
    public FSM() { }
    public FSM(IState<T> curr)
    {
        SetInit(curr);
    }
    public void SetInit(IState<T> curr)
    {
        curr.StateMachine = this;
        _currState = curr;
        _currState.Enter();
    }
    public void OnExecute()
    {
        if (_currState != null)
            _currState.Execute();
    }
    public void OnFixExecute()
    {
        if (_currState != null)
            _currState.FixExecute();
    }
    public void Transition(T input)
    {
        IState<T> newState = _currState.GetTransition(input);
        if (newState == null) return;
        newState.StateMachine = this;
        _currState.Exit();
        _currState = newState;
        _currState.Enter();
    }
}
