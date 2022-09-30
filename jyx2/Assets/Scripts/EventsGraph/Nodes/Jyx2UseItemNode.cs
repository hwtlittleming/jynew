using System;
using System.Collections;
using System.Collections.Generic;


using Jyx2;
using UnityEngine;
using XNode;

[CreateNodeMenu("流程控制/判断是否使用道具")]
[NodeWidth(200)]
public class Jyx2UseItemNode : BaseNode
{
	[Output] public Node yes;
	[Output] public Node no;
	
	private void Reset() {
		name = "判断是否使用道具";
	}

	[Header("道具ID")]
	public int itemId;

	protected override string OnPlay()
	{
		var ret = LuaBridge.UseItem(itemId);
		return ret ? nameof(yes) : nameof(no);
	}
}