using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNodeEditor;

public class NodeEditorHelperScene : NodeEditorHelperBase
{
    public NodeEditorHelperScene(NodeEditor nodeEditor) : base(nodeEditor)
    {
    }

    protected override string Field
    {
        get => "sceneId";
    }
    protected override int TextureHeight
    {
        get => 0;
    }
    protected override string PopupTitle
    {
        get => "场景";
    }
    protected override string[] SelectContent
    {
        get => EventsGraphStatic.s_sceneList;
    }
    protected override string PathFormat
    {
        get => "";
    }
}