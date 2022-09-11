

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