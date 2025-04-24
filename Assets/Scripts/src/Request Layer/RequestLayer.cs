using System;
using System.Collections.Generic;
using System.Linq;
using src.Input_Layer;

namespace src.Request_Layer
{
    [Serializable]
    public class RequestLayer
    {
        public ReqPriorityQueue<RequestSO> generatedRequests;
        
        private readonly List<RequestSO> _characterMoveRequests;
        private readonly InputBuffer<DirectionSignal> _dirCache;
        private readonly InputBuffer<AttackSignal> _atkCache;

        public RequestLayer(InputLayer inputLayer, List<RequestSO> characterMoveRequests)
        {
            _dirCache = inputLayer.directionInput;
            _atkCache = inputLayer.attackInput;
            this._characterMoveRequests = characterMoveRequests;
            generatedRequests = new ReqPriorityQueue<RequestSO>();
        }
        
        public void Start()
        {
            for (int i = 0; i < _characterMoveRequests.Count; i++)
            {
                RequestSO request = _characterMoveRequests[i];
                request.Priority = i;
            }
        }

        public void Update()
        {
            GenerateRequests();
            generatedRequests.Update();
        }

        private void GenerateRequests()
        {
            foreach (var request in _characterMoveRequests.Where(CanRequestPerform))
            {
                generatedRequests.Enqueue(request);
                break;
            }
        }

        private bool CanRequestPerform(RequestSO request)
        {
            bool canPerform = request.type switch
            {
                RequestType.Attack => CanAttackRequestPerform(request),
                RequestType.Move => CanMoveRequestPerform(request),
                _ => false
            };
            return canPerform;
        }
        
        /// <summary>
        /// 根据传入的请求检测一定帧数前的方向输入, 判断是否与招式请求内的方向序列匹配
        /// </summary>
        /// <param name="request">招式指令请求</param>
        /// <returns>匹配结果</returns>
        private bool MatchDirectionSignal(RequestSO request)
        {
            bool match = false;
            // 读取窗口，如果窗口内元素数量为-1，说明缓冲区为空
            var count = _dirCache.GetWindowIndices(request.windowLength);
            if (count < 0) return false;

            // 倒序读取招式请求中的方向信号序列，从缓冲区顶部遍历count数量的元素，与请求中的信号比较
            // 如果缓冲区窗口当前信号与请求所需信号匹配，则读取序列下一个信号，直到请求所有信号全部匹配
            int index = request.directionSignals.Count - 1;
            var expected = request.directionSignals[index];
            for (int i = 0; i < count; i++)
            {
                var signal = _dirCache.Read(i);
                // 匹配要求: 输入信号的方向分量包含所需信号, 且输入时间大于所需时间
                if (!signal.Contains(expected.direction)
                    || signal.duration < expected.duration) continue;
                
                // 如果当前输入信号匹配, 读取请求序列下一个信号
                index--;
                if (index < 0)
                {
                    match = true;
                    break;
                }
                expected = request.directionSignals[index];
            }
            
            return match;
        }
        
        /// <summary>
        /// 根据传入的请求检测一定帧数前的攻击输入,判断是否与招式请求内的攻击信号匹配
        /// 如果当前帧没有输入攻击信号,则直接退出
        /// 如果当前攻击信号的输入时长大于1,也直接推出 —— 暂不处理“压键蓄力”情况
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private bool MatchAttackSignal(RequestSO request)
        {
            // 读取窗口,如果缓冲区为空,则直接退出
            bool match = false;
            var count = _atkCache.GetWindowIndices(request.windowLength);
            if (count < 0) 
                return false;
            
            // 读取当前帧的攻击信号输入,如果没有输入信号或信号持续时长大于1,则退出
            AttackSignal currentFrameSignal = _atkCache.Read(0);
            if (currentFrameSignal.attack == Attack.None || currentFrameSignal.duration > 1)
                return false;
            
            int index = request.attackSignals.Count - 1;
            var expected = request.attackSignals[index];
            for (int i = 0; i < count; i++)
            {
                var signal = _atkCache.Read(i);
                // 攻击信号匹配要求与方向信号相同
                if (!signal.Contains(expected.attack) 
                    || signal.duration < expected.duration) continue;
                
                // 读取请求下一个信号
                index--;
                if (index < 0)
                {
                    match = true;
                    break;
                }
                expected = request.attackSignals[index];
            }
            
            return match;
        }
        
        private bool CanMoveRequestPerform(RequestSO request)
        {
            return MatchDirectionSignal(request);
        }

        private bool CanAttackRequestPerform(RequestSO request)
        {
            if (!MatchAttackSignal(request)) 
                return false;
            if (request.directionSignals is null || request.directionSignals.Count == 0)
                return true;
            return MatchDirectionSignal(request);
        }
    }
}