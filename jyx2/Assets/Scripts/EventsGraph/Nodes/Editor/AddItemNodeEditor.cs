using System.Collections;
using System.Collections.Generic;

using Jyx2;
using MK.Toon;
using UnityEditor;
using UnityEngine;
using XNodeEditor;


[CustomNodeEditor(typeof(AddItemNode))]
public class AddItemNodeEditor : NodeEditor
{
    private NodeEditorHelperItem _itemDrawer;

    public override void OnCreate()
    {
        base.OnCreate();
        _itemDrawer = new NodeEditorHelperItem(this);
    }

    public override void OnBodyGUI()
    {
        // Update serialized object's representation
        serializedObject.Update();

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("prev"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("next"));
        _itemDrawer.DrawAll();
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("count"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("isHint"));
        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }
}