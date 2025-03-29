using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(InputLayer))]
public class RequestLayerOld : MonoBehaviour
{
    public InputLayer inputLayer;
    public List<RequestSO> characterMoveRequests;
    public LinkedList<RequestInfo> generatedRequests;
    // 绿冲数据全角色通用
    // 移动和攻击能否同时触发：蓝防和前冲是 不能同时触发的，点按蓝防帧数为 招架12+硬直33，在蓝防的第4帧可以取消绿冲
    // 前冲的第1~3帧可以接收蓝防指令取消绿冲。
    // 蓝防绿冲的前8帧不可取消任何动作，取消绿冲的前9帧不可取消任意动作。两种绿冲的最大持续时间都是45帧
    // 蓝防绿冲到第23帧才能执行 移动行为, 取消绿冲到第24帧才能执行 移动行为
    // 推论: 请求的角度不需要对 移动/攻击 做区分
    private InputBuffer<DirectionSignal> m_dirCache;
    private InputBuffer<AttackSignal> m_atkCache;

    // [SerializeField] private List<RequestSO> generatedRequests;
    private Queue<DirectionSignal> m_dirWindow;
    private Queue<AttackSignal> m_atkWindow;

    //[SerializeField] private List<DirectionSignal> m_dirDebug;
    //[SerializeField] private List<AttackSignal> m_atkDebug;

    private void Start()
    {
        m_atkCache = inputLayer.attackInput;
        m_dirCache = inputLayer.directionInput;
        generatedRequests = new LinkedList<RequestInfo>();
        m_atkWindow = new Queue<AttackSignal>();
        m_dirWindow = new Queue<DirectionSignal>();
    }

    private void Update()
    {
        RequestGenerate();
        RequestLifetimeUpdate();
    }

    private void RequestLifetimeUpdate()
    {
        for (var i = generatedRequests.First; i != null;)
        {
            if (i.Value.lifeTime <= 0)
            {
                var temp = i;
                i = i.Next;
                generatedRequests.Remove(temp);
            }
            else
            {
                i.Value = new RequestInfo(i.Value.request, i.Value.lifeTime - 1);
                //i.Value.UpdateLifeTime();
                i = i.Next;
            }

        }
        // RequestDebug();
    }

    private void RequestGenerate()
    {
        foreach (var request in characterMoveRequests.Where(CanRequestPerform))
        {
            // Debug.LogWarning(request.name);
            generatedRequests.AddLast(new RequestInfo(request, request.lifeTime));
            break;
        }
    }

    private bool CanRequestPerform(RequestSO request)
    {
        bool res = false;
        switch (request.type)
        {
            case RequestType.Attack:
                res = CanAttackRequestPerform(request);
                break;
            case RequestType.Move:
                res = CanMoveRequestPerform(request);
                break;
        }
        return res;
    }

    private bool CanMoveRequestPerform(RequestSO request)
    {
        DirectionSignal thisFrameDir = m_dirCache.Read(0);
        if (thisFrameDir.direction == Direction.Idle)
            return false;

        bool canPerform = false;
        Queue<DirectionSignal> dirWindow = GetInputWindow(m_dirCache, request.windowLength);
        int index = 0;
        DirectionSignal expectedSignal = request.directionSignals[index];
        while (dirWindow.Count > 0)
        {
            DirectionSignal dequeuedSignal = dirWindow.Dequeue();
            if (dequeuedSignal.Contains(expectedSignal.direction) && dequeuedSignal.duration >= expectedSignal.duration)
            {
                index++;
                if (index >= request.directionSignals.Count)
                {
                    canPerform = true;
                    break;
                }
                expectedSignal = request.directionSignals[index];
            }
        }

        return canPerform;
    }

    private bool CanAttackRequestPerform(RequestSO request)
    {
        // 获取当前的攻击输入信号，跳过 “按住按键” 的情况
        AttackSignal attackInput = m_atkCache.Read(0);
        if (attackInput.duration > 1)
            return false;

        // 获取窗口长度的攻击信号, 判断窗口内是否包含请求的攻击指令序列, 不符合要求则直接退出
        bool canPerform = false;
        Queue<AttackSignal> atkWindow = GetInputWindow(m_atkCache, request.windowLength);
        int index = request.attackSignals.Count - 1;          // 保存请求中攻击指令列表的索引
        AttackSignal expectedAtkSignal = request.attackSignals[index];     // 保存攻击指令列表正在参与判断的信号
        while (atkWindow.Count > 0)
        {
            AttackSignal deququedSignal = atkWindow.Dequeue();
            if (deququedSignal.Contains(expectedAtkSignal.attack))
            {
                index--;
                if (index < 0)
                {
                    canPerform = true;
                    // Debug.LogWarning($"攻击指令判定通过：{deququedSignal.attack}, {deququedSignal.duration}");
                    break;
                }
                expectedAtkSignal = request.attackSignals[index];
            }

        }
        if (canPerform == false)
            return false;
        if (request.directionSignals is null || request.directionSignals.Count == 0)
        {
            // Debug.LogWarning(request.requestID);
            return true;
        }


        // 获取窗口长度的方向信号, 判断窗口内是否包含请求的方向指令序列Queue<DirectionSignal> dirWindow = GetInputWindow(m_dirCache, request.windowLength);
        canPerform = false;
        Queue<DirectionSignal> dirWindow = GetInputWindow(m_dirCache, request.windowLength);
        index = request.directionSignals.Count - 1;
        DirectionSignal expectedDirSignal = request.directionSignals[index];
        while (dirWindow.Count > 0)
        {
            DirectionSignal dequeuedSignal = dirWindow.Dequeue();
            if (dequeuedSignal.Contains(expectedDirSignal.direction) && dequeuedSignal.duration >= expectedDirSignal.duration)
            {
                index--;
                if (index < 0)
                {
                    canPerform = true;
                    Debug.LogWarning(request.requestID);
                    break;
                }
                expectedDirSignal = request.directionSignals[index];
            }
        }
        return canPerform;
    }
    /// <summary>
    /// 从输入缓冲区获取判定输入信号窗口, inputCache 为 方向输入和攻击输入的其中一个
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="inputCache"></param>
    /// <param name="windowLength"></param>
    /// <returns></returns>
    private Queue<AttackSignal> GetInputWindow(InputBuffer<AttackSignal> inputCache, int windowLength)
    {
        int rearIndex = 0, length = windowLength;
        var windowBegin = inputCache.Read(rearIndex);
        m_atkWindow.Clear();
        if (windowBegin is not null)
        {
            //Debug.LogWarning(length);
            length -= windowBegin.duration;
            m_atkWindow.Enqueue(windowBegin);
            while (length > 0)
            {
                rearIndex = (rearIndex + 1) % inputCache.Capacity;
                windowBegin = inputCache.Read(rearIndex);
                if (windowBegin is not null)
                {
                    length -= windowBegin.duration;
                    //Debug.LogWarning(length);
                    m_atkWindow.Enqueue(windowBegin);
                }
                else break;
            }
        }
        return m_atkWindow;
    }
    private Queue<DirectionSignal> GetInputWindow(InputBuffer<DirectionSignal> inputCache, int windowLength)
    {
        int rearIndex = 0, length = windowLength;
        var windowBegin = inputCache.Read(rearIndex);
        m_dirWindow.Clear();
        if (windowBegin is not null)
        {
            //Debug.LogWarning(length);
            length -= windowBegin.duration;
            m_dirWindow.Enqueue(windowBegin);
            while (length > 0)
            {
                rearIndex = (rearIndex + 1) % inputCache.Capacity;
                windowBegin = inputCache.Read(rearIndex);
                if (windowBegin is not null)
                {
                    length -= windowBegin.duration;
                    //Debug.LogWarning(length);
                    m_dirWindow.Enqueue(windowBegin);
                }
                else break;
            }
        }
        // InputWindowDebug(m_dirWindow);
        return m_dirWindow;
    }

    private void InputWindowDebug(Queue<DirectionSignal> window)
    {
        string debug = "";
        foreach (var item in window)
        {
            debug += $"{item.direction} {item.duration}\t";
        }
        Debug.Log(debug);
    }
    private void RequestDebug()
    {
        string debug = "";
        foreach (var req in generatedRequests)
        {
            debug += $"{req.request.requestID}: {req.lifeTime}\n";
        }
        Debug.LogWarning(debug);
    }
    
}
