using System;
using System.Collections;
using System.Collections.Generic;
using Jyx2;
using UnityEngine;
using XNode;

[CreateNodeMenu("流程控制/做选择")]
[NodeWidth(256)]
public class MakeChoiceNode : Jyx2BaseNode
{
    //暂时默认最大5个选项
    [Output] public Node a;
    [Output] public Node b;
    [Output] public Node c;
    [Output] public Node d;
    [Output] public Node e;

    public string content;
    public string[] options;
    
    private void Reset() {
        name = "做选择";
    }

    protected override string OnPlay()
    {
        String nextNode = nameof(a);
        int ret = Jyx2LuaBridge.doChoice(content, options);
        if (ret == 1)
        {
            nextNode = nameof(b);
        }else if (ret == 2)
        {
            nextNode = nameof(c);
        }else if (ret == 3)
        {
            nextNode = nameof(d);
        }else if (ret == 4)
        {
            nextNode = nameof(e);
        }

        return nextNode;
    }
}