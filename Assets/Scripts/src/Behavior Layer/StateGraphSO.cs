using System.Collections.Generic;
using src.Behavior_Layer.FTG_StateMachine;
using UnityEngine;
using UnityHFSM;

namespace src.Behavior_Layer
{
    [CreateAssetMenu(menuName = "MyScriptableObject/Character/State Graph")]
    public class StateGraphSO : ScriptableObject
    {
        public List<MoveBehaviorSO> behaviorConfigs;
        public List<TransitionInfo<MoveBehaviorSO>> transitionConfigs;
        public MoveBehaviorSO startState;
        [Tooltip("招式完成时的触发器名称")] public string moveCompleteTrigger;

        public static FTGStateMachine<MoveBehaviorSO> BuildStateMachine(StateGraphSO stateGraph)
        {
            var fsm = new FTGStateMachine<MoveBehaviorSO>();
            BuildStates(fsm, stateGraph);
            BuildTransitions(fsm, stateGraph);
            fsm.SetStartState(stateGraph.startState);
            return fsm;
        }

        private static void BuildStates(FTGStateMachine<MoveBehaviorSO> fsm, StateGraphSO stateGraph)
        {
            var behaviorConfigs = stateGraph.behaviorConfigs;
            var factory = new StateFactory();
            foreach (var config in behaviorConfigs)
            {
                var newState = factory.Create(config, stateGraph.moveCompleteTrigger);
                fsm.AddState(config, newState);
            }
        }

        private static void BuildTransitions(FTGStateMachine<MoveBehaviorSO> fsm, StateGraphSO stateGraph)
        {
            foreach (var config in stateGraph.transitionConfigs)
            {
                // TODO: 处理源状态为 ANY 的转换
                
                // 校验状态机中是否存在这两个状态，防止名称输入错误
                var fromState = fsm.GetState(config.from);
                var toState = fsm.GetState(config.to);
                
                fsm.AddTriggerTransition(config.trigger, 
                    new Transition<MoveBehaviorSO>(config.from, config.to));
            }
        }
    }
}