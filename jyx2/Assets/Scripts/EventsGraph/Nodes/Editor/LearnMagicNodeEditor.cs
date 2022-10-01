using System.Collections;
using System.Collections.Generic;

using Jyx2;
using MK.Toon;
using UnityEditor;
using UnityEngine;
using XNodeEditor;


[CustomNodeEditor(typeof(LearnMagicNode))]
public class LearnMagicNodeEditor : NodeEditor
{
    private NodeEditorHelperRole _roleDrawer;
    private NodeEditorHelperSkill _skillDrawer;
    public override void OnCreate()
    {
        base.OnCreate();
        _roleDrawer = new NodeEditorHelperRole(this);
        _skillDrawer = new NodeEditorHelperSkill(this);
    }

    public override void OnBodyGUI() 
    {    
        // Update serialized object's representation
        serializedObject.Update();
        
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("prev"));
        _roleDrawer.DrawAll();
        _skillDrawer.DrawField();
        _skillDrawer.DrawPopup();
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("visible"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("next"));
        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

}