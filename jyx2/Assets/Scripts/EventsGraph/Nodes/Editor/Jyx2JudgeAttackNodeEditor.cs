using System.Collections;
using System.Collections.Generic;

using Jyx2;
using MK.Toon;
using UnityEditor;
using UnityEngine;
using XNodeEditor;


[CustomNodeEditor(typeof(Jyx2JudgeAttackNode))]
public class Jyx2JudgeAttackNodeEditor : NodeEditor
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
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("maxValue"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("minValue"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("yes"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("no"));
        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

}