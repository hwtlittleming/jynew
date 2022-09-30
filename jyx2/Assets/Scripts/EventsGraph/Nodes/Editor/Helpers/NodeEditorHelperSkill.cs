using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNodeEditor;

public class NodeEditorHelperSkill : NodeEditorHelperBase
{
    public NodeEditorHelperSkill(NodeEditor nodeEditor) : base(nodeEditor)
    {
    }

    protected override string Field
    {
        get => "skillId";
    }
    protected override int TextureHeight
    {
        get => 0;
    }
    protected override string PopupTitle
    {
        get => "武功";
    }
    protected override string[] SelectContent
    {
        get => EventsGraphStatic.s_skillList;
    }
    protected override string PathFormat
    {
        get => "";
    }
}