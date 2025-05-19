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
    public CharacterMotor motor;
    
    public CharacterEventManager EventManager { get; private set; }
    
    // Config
    [Tooltip("角色招式输入配置，列表索引越小优先级越高")] 
    public List<RequestSO> characterMoveRequests;
    [Tooltip("角色行为状态图配置")] 
    public StateGraphSO stateGraphConfig; 
    
    
    [NonSerialized] public InputLayer inputLayer;
    [NonSerialized, OdinSerialize] public RequestLayer requestLayer;
    [NonSerialized, OdinSerialize] public BehaviorLayer behaviorLayer;

    [NonSerialized, OdinSerialize] public ContextData context;
    
    private void Awake()
    {
        // 获取组件引用
        animationController ??= GetComponent<AnimationController>();
        motor ??= GetComponent<CharacterMotor>();
        EventManager = new CharacterEventManager();
        
        // 初始化 ContextData
        context = new ContextData(player, opponent, animationController, motor);
        
        // 初始化
        inputLayer = new InputLayer(context);
        requestLayer = new RequestLayer(inputLayer, characterMoveRequests);
        behaviorLayer = new BehaviorLayer(this);
        
    }

    private void Start()
    {
        inputLayer.Start();
        requestLayer.Start();
        behaviorLayer.Start();
    }

    private void Update()
    {
        context.isGrounded = motor.IsGrounded;
        inputLayer.Update();
        requestLayer.Update();
        behaviorLayer.Update();
        
    }
}
