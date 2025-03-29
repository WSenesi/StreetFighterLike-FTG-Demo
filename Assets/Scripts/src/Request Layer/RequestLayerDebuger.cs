using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestLayerDebuger : MonoBehaviour
{
    public RequestLayerOld requestLayer;
    public List<RequestInfo> requestDebuger;
    private LinkedList<RequestInfo> requestInfos;
    private int count;

    private void Start()
    {
        requestInfos = requestLayer.generatedRequests;
        requestDebuger = new List<RequestInfo>();
        count = requestInfos.Count;
    }

    private void Update()
    {
        if (count != requestInfos.Count)
        {
            UpdateList();
        }
    }

    private void UpdateList()
    {
        requestDebuger.Clear();
        foreach (var item in requestInfos)
        {
            requestDebuger.Add(item);
            Debug.LogError($"{item.request.name}: {item.lifeTime}");
        }
        count = requestInfos.Count;
    }
}
