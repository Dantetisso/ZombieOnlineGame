using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState<T>
{
    void Initialize(params object[] p);
    void Enter();
    void Execute();
    void FixExecute();
    void Exit();
    IState<T> GetTransition(T input);
    void AddTransition(T input, IState<T> state);
    void RemoveTransition(T input);
    void RemoveTransition(IState<T> state);
    public FSM<T> StateMachine { get; set; }
}
