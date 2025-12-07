using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IFSMState
{

    public void OnEnter();
    public void OnExit();
    public void Update();
}

public class FSMActionExecState : IFSMState
{
    public Action Enter;
    public Action Exit;
    public Action UpdateAction;
    public void OnEnter()
    {
        Enter?.Invoke();
    }
    public void OnExit()
    {
        Exit?.Invoke();
    }
    public void Update()
    {
        UpdateAction?.Invoke();
    }
}
public class TransitionCondition<T> where T : IFSMState
{
    public T ToState { get; private set; }
    public System.Func<bool> Condition { get; private set; }
    public TransitionCondition(T toState, System.Func<bool> condition)
    {
        ToState = toState;
        Condition = condition;
    }
}
public class FSM<T> where T : IFSMState{

    private readonly Dictionary<T, List<TransitionCondition<T>>> transitions = new();
    private T currentState = default;
    public T CurrentState {
        get { return currentState; }
        set {
            AddState(value);
            currentState?.OnExit();
            currentState = value;
            value?.OnEnter();
        }
    }
    public void AddState(T state)
    {
        if (!transitions.ContainsKey(state))
        {
            transitions[state] = new ();
        }
    }

    public void AddTransition(T fromState, T toState, System.Func<bool> condition)
    {
        AddState(fromState);
        transitions[fromState].Add(new TransitionCondition<T>(toState,condition));
    }
    public void Update()
    {
        if (currentState == null) return;
        currentState.Update();
        if (transitions.ContainsKey(currentState))
        {
            foreach (var transition in transitions[currentState])
            {
                if (transition.Condition())
                {
                    CurrentState = transition.ToState;
                    break;
                }
            }
        }
    }


}
