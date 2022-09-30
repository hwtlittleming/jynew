using System.Collections;
using System.Collections.Generic;

using Jyx2;
using MK.Toon;
using UnityEditor;
using UnityEngine;
using XNodeEditor;


[CustomNodeEditor(typeof(Jyx2AddHPNode))]
public class Jyx2AddHPNodeEditor : NodeEditor
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
        _roleDrawer.DrawAll();
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("increment"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("next"));
        
        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

}