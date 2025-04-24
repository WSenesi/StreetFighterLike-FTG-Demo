using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using src.Input_Layer;
using UnityEngine;

namespace src.Request_Layer
{
    /// <summary>
    /// 用以描述 请求 触发 行为 的前置条件
    /// </summary>
    public enum RequestState
    {
        Ground,
        Air,
    }

    /// <summary>
    /// 描述请求的类型，不同类型的请求判定方式可能不同
    /// </summary>
    public enum RequestType
    {
        Move = 0,
        Attack = 1,
        Charge = 2,
    }

    [CreateAssetMenu(fileName = "RequestSO", menuName = "MyScriptableObject/Character/Request")]
    public class RequestSO : SerializedScriptableObject, IPrioritizedExpirable
    {
        public string requestID;
        public RequestType type;
        public List<DirectionSignal> directionSignals;// = new List<DirectionSignal>();
        public List<AttackSignal> attackSignals;// = new List<AttackSignal>();
        public int windowLength = 20;
        public int lifeTime = 10;
        public int priority = 0;
        public List<MappingRuleSO> mappingRules;

        public string Name { get => name; }
        public int Priority { get => priority; set => priority = value; }
        public int Lifetimes { get => lifeTime; set => lifeTime = value; }
    }
    [Serializable]
    public struct RequestInfo
    {
        public RequestSO request;
        public int lifeTime;

        public RequestInfo(RequestSO request, int lifeTime)
        {
            this.request = request;
            this.lifeTime = lifeTime;
        }
        //public void UpdateLifeTime()
        //{
        //    lifeTime -= 1;
        //}
    }
}