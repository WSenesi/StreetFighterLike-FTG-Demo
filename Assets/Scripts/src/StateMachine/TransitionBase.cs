using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class Condition
//{

//    public Condition()
//    {

//    }
//    public bool isTrue()
//    {
//        return
//    }
//}

public class TransitionBase<TStateId>
{
    public TStateId from;
    public TStateId to;

    public FSMStateMachine<TStateId> fsm;

    public TransitionBase(TStateId from, TStateId to)
    {
        this.from = from;
        this.to = to;
    }

    public virtual void Init()
    {

    }

    public virtual void OnEnter()
    {

    }

    public virtual void BeforeTransition()
    {

    }
    
    public virtual void AfterTransition()
    {

    }
}
