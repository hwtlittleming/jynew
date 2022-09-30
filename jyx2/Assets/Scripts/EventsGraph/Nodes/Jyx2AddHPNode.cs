using System;
using System.Collections;
using System.Collections.Generic;


using Jyx2;
using UnityEngine;
using XNode;

[CreateNodeMenu("游戏数据/加血")]
[NodeWidth(200)]
public class Jyx2AddHPNode : SimpleNode
{
	private void Reset() {
		name = "加血";
	}

	[Header("角色id")]
	public int roleId;
    [Header("增加值")]
	public int increment;


	protected override void DoExecute()
	{
		LuaBridge.AddHp(roleId, increment);
	}
}