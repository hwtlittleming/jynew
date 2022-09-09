/*
 * 金庸群侠传3D重制版
 * https://github.com/jynew/jynew
 *
 * 这是本开源项目文件头，所有代码均使用MIT协议。
 * 但游戏内资源和第三方插件、dll等请仔细阅读LICENSE相关授权协议文档。
 *
 * 金庸老先生千古！
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Jyx2;
using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using Jyx2Configs;
using UnityEngine.Playables;

//待重构
public class StoryEngine : MonoBehaviour
{
    public static StoryEngine Instance;


    public bl_HUDText HUDRoot;
    

    public bool BlockPlayerControl
    {
        get { return _blockPlayerControl; }
        set { _blockPlayerControl = value; }
    }


    private bool _blockPlayerControl;

    private static GameRuntimeData runtime
    {
        get { return GameRuntimeData.Instance; }
    }

    private void Awake()
    {
        Instance = this;
    }

    public async void DisplayPopInfo(string msg, float duration = 2f)
    {
        await Jyx2_UIManager.Instance.ShowUIAsync(nameof(CommonTipsUIPanel), TipsType.Common, msg, duration);
    }

    
}