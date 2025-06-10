using System;
using System.Collections.Generic;
using src.Request_Layer;
using UnityEngine;

public delegate bool RequestCheck<in TRequest, in TContext, TMappingRule>(
    TRequest request, TContext context, out TMappingRule mappingRule);

/// <summary>
/// 支持元素过期销毁的优先队列。
/// 由于队列元素需要根据生命周期从队列中销毁，且元素具有优先级，为了防止低优先级元素被高优先级元素“堵塞”，每帧会遍历清除队列中所有过期元素
/// 涉及频繁删除元素，因此使用双向链表实现。
/// 清除过期元素时间复杂度为O(1),从队头出队时间复杂度为O(1)
/// 根据优先级进入队列时间复杂度为O(N)
/// (泛型接口还有优化空间) 
/// </summary>
/// <typeparam name="TRequest">包含优先级和生命周期数据的请求</typeparam>
[Serializable]
public class ReqPriorityQueue<TRequest> where TRequest : IPrioritizedExpirable
{
    private class RequestNode
    {
        public TRequest request;
        public int lifetimes;
        public RequestNode prev, next;

        public void Reset()
        {
            request = default(TRequest);
            lifetimes = 0;
            prev = next = null;
        }
    }

    [Serializable]
    private struct RequestNodeInfo
    {
        public string name;
        public int priority;
        public int remainingFrames;
    }
    
    [SerializeField] private List<RequestNodeInfo> _debugNodes = new List<RequestNodeInfo>();
    [SerializeField] private int _activeNodes = 0;
    [SerializeField] private int _pooledNodes = 0;
    
    private Stack<RequestNode> _nodePool = new Stack<RequestNode>();
    private RequestNode _head, _tail;

    public void Update()
    {
        RequestNode current = _head;
        while (current is not null)
        {
            RequestNode next = current.next;
            current.lifetimes -= 1;
            if (current.lifetimes <= 0)
            {
                RemoveNode(current);
                ReleaseNode(current);
            }
            current = next;
        }
        
        UpdateNodeInfo();
    }
    
    public void Enqueue(TRequest request)
    {
        int priority = request.Priority;
        var newNode = GetNode();
        newNode.request = request;
        newNode.lifetimes = request.Lifetimes;

        var currentNode = _head;
        while (currentNode is not null && currentNode.request.Priority < priority)
            currentNode = currentNode.next;
        
        InsertNodeBefore(currentNode, newNode);
    }

    public bool TryDequeue(out TRequest request)
    {
        if (_head is not null)
        {
            var nodeToRelease = _head;
            request = _head.request;
            RemoveNode(nodeToRelease);
            ReleaseNode(nodeToRelease);
            return true;
        }
        
        request = default(TRequest);
        return false;
    }

    /// <summary>
    /// 按优先级顺序查找第一个满足指定外部条件的请求。
    /// 条件检查逻辑由外部通过 conditionCheck 委托提供，该委托还需要检查请求内部的映射规则。
    /// 如果找到，则将请求从队列中移除，并返回该请求及触发它的映射规则。
    /// </summary>
    /// <typeparam name="TContext">上下文数据的类型。</typeparam>
    /// <typeparam name="TMappingRule">映射规则的类型。</typeparam>
    /// <param name="context">传递给条件检查委托的上下文数据。</param>
    /// <param name="condition">
    /// 一个委托，用于判断给定的请求和上下文是否满足条件。
    /// 如果满足，该委托应将匹配的映射规则赋值给其 out 参数并返回 true。
    /// </param>
    /// <param name="request">如果找到满足条件的请求，则返回该请求；否则返回 default。</param>
    /// <param name="matchedRule">如果找到满足条件的请求，则返回触发该请求的映射规则；否则返回 default。</param>
    /// <returns>如果找到并移除了满足条件的请求，则返回 true；否则返回 false。</returns>
    public bool TryDequeueIf<TContext, TMappingRule>(
        TContext context,
        RequestCheck<TRequest, TContext, TMappingRule> condition, 
        out TRequest request,
        out TMappingRule matchedRule) where TMappingRule : MappingRuleSO
    {
        var current = _head;
        while (current is not null)
        {
            // 调用外部传入的委托进行检查
            // 委托内部会处理遍历 MappingRuleSO 列表和 ConditionBase 列表的逻辑
            if (condition(current.request, context, out var currentMatchedRule))
            {
                // 委托返回 true，表示找到了满足条件的请求和规则
                request = current.request;
                matchedRule = currentMatchedRule; // 获取由委托找到的规则

                // Debug.Log($"Dequeuing request '{request.Name}' triggered by rule '{matchedRule?.name}' (or identifier)."); // 可选调试

                RemoveNode(current);  // 从链表中移除
                ReleaseNode(current); // 回收到对象池
                UpdateNodeInfo();     // 更新调试信息
                return true;          // 成功找到并移除，返回 true
            }

            // 如果当前请求不满足条件，继续检查下一个 (优先级更低的)
            current = current.next;
        }

        // 遍历完所有请求都没有找到满足条件的
        request = default;
        matchedRule = null;
        return false; // 返回 false
    }
    
    private RequestNode GetNode()
    {
        return _nodePool.Count > 0 ? _nodePool.Pop() : new RequestNode();
    }

    private void ReleaseNode(RequestNode node)
    {
        node.Reset();
        _nodePool.Push(node);
    }
    
    private void InsertNodeBefore(RequestNode target, RequestNode newNode)
    {
        // 3种特殊情况: 空队列, 插入位置为队尾或队首
        if (target is null)
        {
            // 空队列
            if (_tail is null)
            {
                _head = _tail = newNode;
            }
            // 插入到队尾
            else
            {
                _tail.next = newNode;
                newNode.prev = _tail;
                _tail = newNode;
            }
        }
        else
        {
            newNode.prev = target.prev;
            newNode.next = target;
            // 插入到队首
            if (target.prev is null)
                _head = newNode;
            else target.prev.next = newNode;
            target.prev = newNode;
        }
    }

    private void RemoveNode(RequestNode node)
    {
        // 两种特殊情况: 移除队首/队尾的元素,即node的前/后元素为空
        if (node.prev is null)
            _head = node.next;
        else node.prev.next = node.next;
        
        if (node.next is null)
            _tail = node.prev;
        else node.next.prev = node.prev;
    }

    private void UpdateNodeInfo()
    {
        _debugNodes.Clear();
        RequestNode current = _head;
        while (current is not null)
        {
            TRequest req = current.request;
            _debugNodes.Add(new RequestNodeInfo()
            {
                name = req.Name,
                priority = req.Priority,
                remainingFrames = current.lifetimes,
            });
            current = current.next;
        }
        
        _activeNodes = _debugNodes.Count;
        _pooledNodes = _nodePool.Count;
    }
}
