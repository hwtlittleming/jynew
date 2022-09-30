using System;
using System.Collections;
using System.Collections.Generic;
using Jyx2;
using UnityEngine;

[CreateNodeMenu("场景/移动主角")]
[NodeWidth(150)]
public class Jyx2MovePlayerNode : SimpleNode
{
    private void Reset() {
        name = "移动主角";
    }

    [Header("物体路径")] public string objPath;
    [Header("根目录")] public string parentDir;
    
    protected override void DoExecute()
    {
        LuaBridge.jyx2_MovePlayer(objPath, parentDir);
    }
}
