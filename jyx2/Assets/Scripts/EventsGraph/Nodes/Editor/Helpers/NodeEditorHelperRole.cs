using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNodeEditor;

public class NodeEditorHelperRole : NodeEditorHelperBase
{
    public NodeEditorHelperRole(NodeEditor nodeEditor) : base(nodeEditor)
    {
    }

    protected override string Field
    {
        get => "roleId";
    }

    protected override int TextureHeight
    {
        get => 50;
    }
    protected override string PopupTitle
    {
        get => "角色";
    }
    protected override string[] SelectContent
    {
        get => EventsGraphStatic.s_roleList;
    }
    protected override string PathFormat
    {
        get => "Assets/BuildSource/head/{0}.png";
    }
}