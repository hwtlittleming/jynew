using System;
using System.Collections;
using System.Collections.Generic;


using Jyx2;
using UnityEngine;
using XNode;

[CreateNodeMenu("场景/摄像机锁定到")]
[NodeWidth(300)]
public class CameraFollowNode : SimpleNode
{
    private void Reset() {
        name = "摄像机锁定到";
    }
    [Header("角色/物体路径")]
    public string path;
    
    protected override void DoExecute()
	{
		LuaBridge.jyx2_CameraFollow(path);
	}
}
