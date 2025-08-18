using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State<T> : IState<T>
{
    FSM<T> _fsm;
    Dictionary<T, IState<T>> _transitions = new Dictionary<T, IState<T>>();

    public virtual void Initialize(params object[] p)
    {
    }
    public virtual void Enter()
    {
    }
    public virtual void Execute()
    {
    }
    public virtual void FixExecute()
    {
    }
    public virtual void Exit()
    {
    }
    public IState<T> GetTransition(T input)
    {
        if (!_transitions.ContainsKey(input)) return null;
        return _transitions[input];
    }
    public void AddTransition(T input, IState<T> state)
    {
        _transitions[input] = state;
    }
    public void RemoveTransition(T input)
    {
        if (_transitions.ContainsKey(input))
        {
            _transitions.Remove(input);
        }
    }
    public void RemoveTransition(IState<T> state)
    {
        foreach (var item in _transitions)
        {
            if (item.Value == state)
            {
                _transitions.Remove(item.Key);
                break;
            }
        }
    }
    public FSM<T> StateMachine
    {
        set
        {
            _fsm = value;
        }
        get
        {
            return _fsm;
        }
    }
}
