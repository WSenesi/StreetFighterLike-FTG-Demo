using System;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    public void OnEnter();
    public void OnExecution();
    public void OnExit();
}

public class FSMState<TStateId> : IState
{
    public TStateId name;
    /// <summary>
    /// 标识当前 状态/子状态机 隶属于哪一个状态机
    /// </summary>
    public FSMStateMachine<TStateId> stateMachine;
    public bool isActive;
    public List<TransitionBase<TStateId>> transitions;

    private Action<FSMState<TStateId>> m_onExecution;
    private Action<FSMState<TStateId>> m_onEnter;
    private Action<FSMState<TStateId>> m_onExit;

    public FSMState(Action<FSMState<TStateId>> onEnter = null, Action<FSMState<TStateId>> onExecution = null, Action<FSMState<TStateId>> onExit = null)
    {
        m_onEnter = onEnter;
        m_onExecution = onExecution;
        m_onExit = onExit;
    }
    public virtual void AddTransition(TransitionBase<TStateId> transition)
    {
        transitions ??= new List<TransitionBase<TStateId>>();
        transitions.Add(transition);
    }
    public virtual void OnInit()
    {

    }
    public virtual void OnEnter()
    {
        m_onEnter?.Invoke(this);
    }

    public virtual void OnExecution()
    {
        m_onExecution?.Invoke(this);
    }

    public virtual void OnExit()
    {
        m_onExit?.Invoke(this);
    }

}
