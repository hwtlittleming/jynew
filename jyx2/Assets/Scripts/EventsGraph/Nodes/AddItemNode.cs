﻿using System;
using System.Collections;
using System.Collections.Generic;


using Jyx2;
using UnityEngine;
using XNode;

[CreateNodeMenu("游戏数据/增减道具")]
[NodeWidth(200)]
public class AddItemNode : SimpleNode
{
	private void Reset() {
		name = "增减道具";
	}

	[Header("道具ID")] public int itemId;
	[Header("数量")] public int count;
	[Header("是否提示")] public bool isHint;


	protected override void DoExecute()
	{
		LuaBridge.AddItem(itemId, count,0,isHint);
	}
}