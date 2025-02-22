﻿

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu]
public class Jyx2NodeGraph : NodeGraph { 
    private Action _callback = null;
     
     /// <summary>
     /// 运行一个nodeGraph
     /// </summary>
     public void Run(Action callback)
     {
         _callback = callback;
         
         var startNode = FindStartNode();
 
         if (startNode == null)
         {
             Debug.LogError("找不到开始节点!需要有一个没有prev连线的节点作为开始节点！");
             return;
         }
         
         Debug.Log($"Jyx2NodeGraph {name} 开始执行");
 
         PlayNextNode(startNode);
     }
 
     void PlayNextNode(Node currentNode)
     {
         if (currentNode == null)
         {
             Debug.Log($"Jyx2NodeGraph {name} 执行完毕");
             _callback ? .Invoke();
             return;
         }
 
         var node = currentNode as BaseNode;
         if (node == null)
         {
             Debug.LogError("执行错误：有节点不是派生自BaseNode！");
             return;
         }
 
         //Debug.Log($"Node {node.name} is playing");
 
         Loom.RunAsync(() =>
         {
             var nextNode = node.Play();
             
             Loom.QueueOnMainThread(_ =>
             {
                 PlayNextNode(nextNode);    
             }, null);
         });
     }
 
     Node FindStartNode()
     {
         foreach (var node in nodes)
         {
             if (node == null)
                 continue;
             
             if (node.GetInputValue<Node>("prev", null) == null)
             {
                 return node;
             }
         }
 
         return null;
     }
}