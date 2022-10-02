using System.Collections;
using System.Collections.Generic;

using Jyx2;
using MK.Toon;
using UnityEditor;
using UnityEngine;
using XNodeEditor;


[CustomNodeEditor(typeof(ReplaceSceneObjNode))]
public class ReplaceSceneObjNodeEditor : NodeEditor
{
    private NodeEditorHelperScene _sceneDrawer;

    public override void OnCreate()
    {
        base.OnCreate();
        _sceneDrawer = new NodeEditorHelperScene(this);
    }

    public override void OnBodyGUI() 
    {    
        // Update serialized object's representation
        serializedObject.Update();
        
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("prev"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("next"));
        _sceneDrawer.DrawField();
        _sceneDrawer.DrawPopup();
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("isShow"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("path"));
        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

}