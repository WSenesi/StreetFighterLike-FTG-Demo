using System.Collections.Generic;
using Sirenix.OdinInspector;
using src.Behavior_Layer.FTG_StateMachine;
using src.Input_Layer;
using UnityEngine;
using UnityHFSM;

namespace src.Behavior_Layer
{
    [CreateAssetMenu(menuName = "Character/State Graph")]
    public class StateGraphSO : SerializedScriptableObject
    {
        [Header("基础移动行为")]
        public IdleConfigSO idle;
        public MovementConfigSO walkForward, walkBackward;
        public CrouchConfigSO crouch;
        public JumpConfigSO jumpNeutral, jumpForward, jumpBackward;
        
        [Header("附加招式")]
        public List<BaseBehaviorConfigSO> behaviorConfigs;
        [Header("附加招式转换")]
        public List<TransitionInfo<BaseBehaviorConfigSO>> transitionConfigs;
        // [Header("默认状态")]
        // public BaseBehaviorConfigSO defaultState;
        [Tooltip("招式完成时的触发器名称")] public string moveCompleteTrigger;

        public static FTGStateMachine<BaseBehaviorConfigSO> BuildStateMachine(
            Character character, StateGraphSO stateGraph)
        {
            var fsm = new FTGStateMachine<BaseBehaviorConfigSO>(character);
            BuildStates(fsm, stateGraph);
            BuildBasicTransitions(fsm, stateGraph);
            BuildAdditionalTransitions(fsm, stateGraph);
            // 创建行为结束时返回至Idle的转换
            fsm.AddTriggerTransitionFromAny(stateGraph.moveCompleteTrigger, 
                new Transition<BaseBehaviorConfigSO>(null, stateGraph.idle));
            fsm.SetStartState(stateGraph.idle);
            return fsm;
        }

        private static void BuildStates(FTGStateMachine<BaseBehaviorConfigSO> fsm, StateGraphSO stateGraph)
        {
            var factory = new StateFactory(stateGraph.idle, stateGraph.moveCompleteTrigger);
            // 创建基础行为状态
            fsm.AddState(stateGraph.idle, factory.Create(stateGraph.idle));
            fsm.AddState(stateGraph.walkForward, factory.Create(stateGraph.walkForward));
            fsm.AddState(stateGraph.walkBackward, factory.Create(stateGraph.walkBackward));
            fsm.AddState(stateGraph.crouch, factory.Create(stateGraph.crouch));
            fsm.AddState(stateGraph.jumpNeutral, factory.Create(stateGraph.jumpNeutral));
            fsm.AddState(stateGraph.jumpForward, factory.Create(stateGraph.jumpForward));
            fsm.AddState(stateGraph.jumpBackward, factory.Create(stateGraph.jumpBackward));
            
            // 创建附加招式状态
            var behaviorConfigs = stateGraph.behaviorConfigs;
            foreach (var config in behaviorConfigs)
            {
                var newState = factory.Create(config);
                fsm.AddState(config, newState);
            }
        }

        private static void BuildAdditionalTransitions(FTGStateMachine<BaseBehaviorConfigSO> fsm, StateGraphSO stateGraph)
        {
            foreach (var config in stateGraph.transitionConfigs)
            {
                // 校验状态机中是否存在这两个状态，防止名称输入错误
                var fromState = fsm.GetState(config.from);
                var toState = fsm.GetState(config.to);
                
                fsm.AddTriggerTransition(config.trigger, 
                    new Transition<BaseBehaviorConfigSO>(config.from, config.to));
            }
        }

        private static void BuildBasicTransitions(FTGStateMachine<BaseBehaviorConfigSO> fsm, StateGraphSO stateGraph)
        {
            // Idle 到 下蹲、跳跃、移动
            fsm.AddTransition(
                 from: stateGraph.idle,
                 to: stateGraph.crouch,
                 condition: (transition) => ContainsDirection(fsm.ContextData.dirInput, Direction.Down)
             );
            fsm.AddTransition(
                from: stateGraph.idle,
                to: stateGraph.walkForward,
                condition: (transition) => ContainsDirection(fsm.ContextData.dirInput, Direction.Front)
            );
            fsm.AddTransition(
                from: stateGraph.idle,
                to: stateGraph.walkBackward,
                condition: (transition) => ContainsDirection(fsm.ContextData.dirInput, Direction.Back)
            );
            fsm.AddTransition(
                from: stateGraph.idle,
                to: stateGraph.jumpForward,
                condition: (transition) => ContainsDirection(fsm.ContextData.dirInput, Direction.Up | Direction.Front)
                );
            fsm.AddTransition(
                from: stateGraph.idle,
                to: stateGraph.jumpBackward,
                condition: (transition) => ContainsDirection(fsm.ContextData.dirInput, Direction.Up | Direction.Back)
            );
            fsm.AddTransition(
                from: stateGraph.idle,
                to: stateGraph.jumpNeutral,
                condition: (transition) => ContainsDirection(fsm.ContextData.dirInput, Direction.Up)
            );

            
            // 移动 到 跳跃、下蹲、Idle
            fsm.AddTransition(
                from: stateGraph.walkForward,
                to: stateGraph.idle,
                condition: (transition) => !ContainsDirection(fsm.ContextData.dirInput, Direction.Front)
            );
            fsm.AddTransition(
                from: stateGraph.walkForward,
                to: stateGraph.jumpForward,
                condition: (transition) => ContainsDirection(fsm.ContextData.dirInput, Direction.Front | Direction.Up)
            );
            fsm.AddTransition(
                from: stateGraph.walkForward,
                to: stateGraph.crouch,
                condition: (transition) => ContainsDirection(fsm.ContextData.dirInput, Direction.Down)
            );
            
            fsm.AddTransition(
                from: stateGraph.walkBackward,
                to: stateGraph.idle,
                condition: (transition) => !ContainsDirection(fsm.ContextData.dirInput, Direction.Back)
            );
            fsm.AddTransition(
                from: stateGraph.walkBackward,
                to: stateGraph.jumpBackward,
                condition: (transition) => ContainsDirection(fsm.ContextData.dirInput, Direction.Back | Direction.Up)
            );
            fsm.AddTransition(
                from: stateGraph.walkBackward,
                to: stateGraph.crouch,
                condition: (transition) => ContainsDirection(fsm.ContextData.dirInput, Direction.Down)
            );
            
            
            // 下蹲 到 Idle
            fsm.AddTransition(
                from: stateGraph.crouch,
                to: stateGraph.idle,
                condition: (transition) => !ContainsDirection(fsm.ContextData.dirInput, Direction.Down)
            );
        }

        private static bool ContainsDirection(Direction input, Direction required)
        {
            if (required == Direction.None)
                return input == Direction.None;
            return (input & required) == required;
        }
    }
}