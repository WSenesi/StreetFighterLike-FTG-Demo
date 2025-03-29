using System.Collections.Generic;
using src.Request_Layer;
using UnityEngine;

/// <summary>
/// 支持元素过期销毁的优先队列。
/// 由于队列元素需要根据生命周期从队列中销毁，且元素具有优先级，为了防止低优先级元素被高优先级元素“堵塞”，每帧会遍历清除队列中所有过期元素
/// 涉及频繁删除元素，因此使用双向链表实现。
/// 清除过期元素时间复杂度为O(1),从队头出队时间复杂度为O(1)
/// 根据优先级进入队列时间复杂度为O(N)
/// (泛型接口还有优化空间) 
/// </summary>
/// <typeparam name="TRequest">包含优先级和生命周期数据的请求</typeparam>
[System.Serializable]
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

    [System.Serializable]
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
            request = _head.request;
            RemoveNode(_head);
            ReleaseNode(_head);
            return true;
        }
        
        request = default(TRequest);
        return false;
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
