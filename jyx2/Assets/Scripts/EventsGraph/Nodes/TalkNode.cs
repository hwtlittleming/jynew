using System;
using System.Collections;
using System.Collections.Generic;


using Jyx2;
using UnityEngine;
using XNode;

[CreateNodeMenu("对话")]
[NodeWidth(256)]
public class TalkNode : SimpleNode
{

	private void Reset() {
		name = "对话";
	}
	
	public String roleId;
	public string talkerName;
	public string content;
	
	// Use this for initialization
	protected override void Init() {
		base.Init();
		
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port) {
		return null; // Replace this
	}
	
	protected override void DoExecute()
	{
		LuaBridge.Talk(roleId, content, talkerName);//talker如果传值则覆盖名称，即不用专门配一个角色来说话
	}
}