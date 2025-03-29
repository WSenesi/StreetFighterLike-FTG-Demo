using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateMachine<TStateId>
{
    FSMState<TStateId> ActiveState { get; set; }
    TStateId ActiveStateName { get; }
    FSMState<TStateId> LastState { get; set; }
    void SwitchState(TStateId name);
}
public class FSMStateMachine<TStateId> : FSMState<TStateId>, IStateMachine<TStateId>
{
    public FSMState<TStateId> ActiveState { get; set; }

    public TStateId ActiveStateName { get => ActiveState.name; }

    public FSMState<TStateId> LastState { get; set; }
    
    public FSMState<TStateId> DefaultState { get; set; }

    public Dictionary<TStateId, FSMState<TStateId>> StateDict = new Dictionary<TStateId, FSMState<TStateId>>();

    public bool IsRootMachine { get => stateMachine == null; }

    public void SwitchState(TStateId name)
    {
        throw new System.NotImplementedException();
    }
    public void AddState(TStateId name, FSMState<TStateId> state)
    {
        state.stateMachine = this;
        state.name = name;
        state.OnInit();

        if (StateDict.Count == 0)
            DefaultState = state;

        StateDict.Add(name, state);
    }
    // 如果是根状态机，调用OnEnter()进入流程
    public override void OnInit()
    {
        if (!IsRootMachine) return;
        OnEnter();
    }

    public override void OnExecution()
    {
        base.OnExecution();

        ActiveState.OnExecution();
    }
}
