using System;
using src.Behavior_Layer.FTG_StateMachine;
using src.PresentationLayer;
using src.Request_Layer;
using UnityEngine;

namespace src.Behavior_Layer
{
    [Serializable]
    public class BehaviorLayer
    {
        public Character Character { get; }
        private StateGraphSO _stateGraphConfig;
    
        // TODO: Other Component
        private CharacterEventManager _eventManager;
        private AnimationController _animationController;
        private CharacterMotor _characterMotor;
        
    
        private ReqPriorityQueue<RequestSO> _generatedRequests;
        private FTGStateMachine<BaseBehaviorConfigSO> _fsm;
        private ContextData _contextData;
        private CharacterContextFlag _currentContextFlag;

        public BehaviorLayer(Character character)
        {
            Character = character;
            _generatedRequests = character.requestLayer.generatedRequests;
            _eventManager = character.EventManager;
            _stateGraphConfig = character.stateGraphConfig;
            _contextData = character.context;
        
            _animationController = _contextData.animationController;
            _characterMotor = _contextData.motor;
        }
    
        public void Start()
        {
            // State 的 Init 方法在状态机 AddState 方法中调用，因此状态订阅事件的逻辑应该在构建状态机之前完成
            _fsm = StateGraphSO.BuildStateMachine(Character, _stateGraphConfig);
            _fsm.ContextData = _contextData;
            _fsm.Init();
            
            // TODO: 订阅事件管理器的 OnRawCollisionsDetected 
        }

        public void Update()
        {
            // 1. 获取当前Context
            // UpdateContextData();
        
            // 2. 尝试从请求队列获取能够执行的状态触发器
            TryGetBehavior();
        
            // 3. 执行状态
            _fsm.OnLogic();
        
            // 设置朝向
            _characterMotor.SetFacingDirection(_contextData.isFacingRight);
        }

        private void UpdateContextData()
        {
            _contextData = _fsm.ContextData;
        }

        private void TryGetBehavior()
        {
            RequestSO request = null;
            MappingRuleSO mappingRule = null;
            if (_generatedRequests.TryDequeueIf(_contextData, CheckRequest, out request, out mappingRule))
            {
                var trigger = mappingRule.mappingResult;
                Debug.Log($"{mappingRule.name}: {trigger}");
                _fsm.Trigger(trigger);
            }
        }

        private bool CheckRequest(RequestSO request, ContextData context, out MappingRuleSO matchedRule)
        {
            // 遍历请求中所有 MappingRule
            foreach (var mappingRule in request.mappingRules)
            {
                // 检查当前mappingRule是否满足条件
                if (mappingRule.MatchesContext(context))
                {
                    matchedRule = mappingRule;
                    return true;
                }
            }
        
            matchedRule = null;
            return false;
        }
    }
}
