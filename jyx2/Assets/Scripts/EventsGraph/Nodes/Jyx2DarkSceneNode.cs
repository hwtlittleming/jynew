using System;
using System.Collections;
using System.Collections.Generic;
using Jyx2;
using UnityEngine;

[CreateNodeMenu("效果/屏幕变暗")]
[NodeWidth(100)]
public class Jyx2DarkScenceNode : SimpleNode
{
    private void Reset() {
        name = "屏幕变暗";
    }

    protected override void DoExecute()
    {
        LuaBridge.DarkScence();
    }
}
