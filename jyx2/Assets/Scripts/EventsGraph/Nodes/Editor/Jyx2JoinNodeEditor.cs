using System.Collections;
using System.Collections.Generic;

using Jyx2;
using MK.Toon;
using UnityEditor;
using UnityEngine;
using XNodeEditor;


[CustomNodeEditor(typeof(Jyx2JoinNode))]
public class Jyx2JoinNodeEditor : NodeEditor
{
    private NodeEditorHelperRole _roleDrawer;

    public override void OnCreate()
    {
        base.OnCreate();
        _roleDrawer = new NodeEditorHelperRole(this);
    }

    public override void OnBodyGUI() 
    {    
        // Update serialized object's representation
        serializedObject.Update();
        
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("prev"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("next"));

        _roleDrawer.DrawAll();
        
        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

}