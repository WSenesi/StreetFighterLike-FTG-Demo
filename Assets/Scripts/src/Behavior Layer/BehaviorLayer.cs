using System;
using src;
using src.Behavior_Layer;
using src.Behavior_Layer.FTG_StateMachine;
using src.Request_Layer;

[Serializable]
public class BehaviorLayer
{
    private StateGraphSO stateGraphConfig;
    
    // TODO: Other Component
    private AnimationController _animationController;
    
    private ReqPriorityQueue<RequestSO> _generatedRequests;
    private FTGStateMachine<MoveBehaviorSO> _fsm;
    private ContextData _contextData;
    private CharacterContextFlag _currentContextFlag;

    public BehaviorLayer(RequestLayer requestLayer, StateGraphSO stateGraphConfig, ContextData contextData)
    {
        _generatedRequests = requestLayer.generatedRequests;
        this.stateGraphConfig = stateGraphConfig;
        this._contextData = contextData;
    }
    
    public void Start()
    {
        _fsm = StateGraphSO.BuildStateMachine(stateGraphConfig);
        _fsm.Init();

        _contextData = _fsm.ContextData;
    }

    public void Update()
    {
        // 1. 获取当前Context
        UpdateContextData();
        
        // 2. 尝试从请求队列获取能够执行的状态触发器
        TryGetBehavior();
        
        // 3. 执行状态
        _fsm.OnLogic();
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
