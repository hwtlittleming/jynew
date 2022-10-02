using System;
using System.Collections;
using System.Collections.Generic;


using Jyx2;
using UnityEditor;
using UnityEngine;
using XNode;

[CreateNodeMenu("事件修改")]
[NodeWidth(200)]
public class Jyx2ModifyEventNode : SimpleNode
{
    private void Reset() {
        name = "事件修改";
    }
    
    public String SceneId = "this";
    public String EventId = "this";
    
    /// 交互事件ID
    public String InteractiveEventId = "-1";
    
    /// 使用道具ID
    public String UseItemEventId = "-1";
    
    /// 进入直接触发事件ID
    public String EnterEventId = "-1";

    protected override void DoExecute()
    {
        LuaBridge.ModifyEvent(SceneId, EventId, InteractiveEventId, UseItemEventId, EnterEventId);
    }
}
