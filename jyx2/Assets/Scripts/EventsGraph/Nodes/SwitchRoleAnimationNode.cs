using System;
using System.Collections;
using System.Collections.Generic;


using Jyx2;
using UnityEngine;
using XNode;

[CreateNodeMenu("场景/切换地图角色动态")]
[NodeWidth(150)]
public class SwitchRoleAnimationNode : SimpleNode
{
	private void Reset() {
		name = "切换地图角色动态";
	}
    
    [Header("角色路径")]
    public string rolePath = "";
    [Header("controller路径")]
    public string animationControllerPath;
    [Header("场景id")]
    public string sceneId;
    
	protected override void DoExecute()

	{
		LuaBridge.SwitchRoleAnimation(rolePath, animationControllerPath, sceneId);
	}
}