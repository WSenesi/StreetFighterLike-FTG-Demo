using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using src;
using src.Behavior_Layer;
using src.Input_Layer;
using src.PresentationLayer;
using src.Request_Layer;
using UnityEngine;

public class Character : SerializedMonoBehaviour
{
    // Component
    public Transform player;
    public Transform opponent;
    public AnimationController animationController;
    
    // Config
    [Tooltip("角色招式输入配置，列表索引越小优先级越高")] 
    public List<RequestSO> characterMoveRequests;
    [Tooltip("角色行为状态图配置")] 
    public StateGraphSO stateGraphConfig; 
    
    
    [NonSerialized] public InputLayer inputLayer;
    [NonSerialized, OdinSerialize] public RequestLayer requestLayer;
    [NonSerialized, OdinSerialize] public BehaviorLayer behaviorLayer;

    [OdinSerialize] private ContextData _context;
    
    private void Awake()
    {
        // 初始化 ContextData
        _context = new ContextData(player, opponent, animationController);
        
        // 初始化三层
        inputLayer = new InputLayer(_context);
        requestLayer = new RequestLayer(inputLayer, characterMoveRequests);
        behaviorLayer = new BehaviorLayer(requestLayer, stateGraphConfig, _context);
        
    }

    private void Start()
    {
        inputLayer.Start();
        requestLayer.Start();
        behaviorLayer.Start();
    }

    private void Update()
    {
        inputLayer.Update();
        requestLayer.Update();
        behaviorLayer.Update();
    }
}
