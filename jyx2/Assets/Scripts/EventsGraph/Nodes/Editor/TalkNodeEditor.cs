using System.Collections;
using System.Collections.Generic;

using Jyx2;
using MK.Toon;
using UnityEditor;
using UnityEngine;
using XNodeEditor;


[CustomNodeEditor(typeof(TalkNode))]
public class TalkNodeEditor : NodeEditor
{
    private NodeEditorHelperRole _roleDrawer;

    public override void OnCreate()
    {
        base.OnCreate();
        _roleDrawer = new NodeEditorHelperRole(this);
        EditorStyles.textField.wordWrap = true; // 自动换行
    }

    public override void OnBodyGUI() {
        // Update serialized object's representation
        serializedObject.Update();
        
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("prev"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("next"));
        
        _roleDrawer.DrawField();
        //_roleDrawer.DrawPopup();
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("talkerName"), new GUIContent("姓名(无图覆盖)"));
        //角色头像
        var roleHeadContent = new GUIContent(_roleDrawer.GetTexture());
        
        EditorGUIUtility.labelWidth = 25.0f; // Replace this with any width
        EditorGUILayout.PropertyField(serializedObject.FindProperty("content"),
            roleHeadContent, GUILayout.MinHeight(40f), GUILayout.MaxHeight(100f));
        
        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }
}