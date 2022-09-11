

using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 游戏的驱动事件
/// </summary>
public class GameEvent : MonoBehaviour
{
    static public GameEvent GetCurrentGameEvent()
    {
        return GameEventManager.GetCurrentGameEvent();
    }

    public const int NO_EVENT = -1;

    /// <summary>
    /// 交互对象
    /// </summary>
    public GameObject[] m_EventTargets;

    /// <summary>
    /// 有哪些事件类型(0:直接触发；1：交谈 2：观察 3：使用物品 4：偷袭)多项则都放进去，观察是通用的
    /// 直接触发的eventId用m_InteractiveEventId
    /// </summary>
    [Header("事件类型(0:直接触发；1：交谈 2：观察 3：使用物品 4：偷袭)多项则都放进去")]
    public String m_EventType = "-1";
    
    /// 交谈事件id
    public int m_InteractiveEventId ;
    
    /// 使用物品事件id (灵活-可下毒，赠送各种效果)
    public int m_UseItemEventId ;
    
    /// 偷袭事件id
    public int m_HitEventId ;
    

    /// <summary>
    /// 交互提示按钮文字
    /// </summary>
    //---------------------------------------------------------------------------
    //public string m_InteractiveInfo = "交互";
    //---------------------------------------------------------------------------
    //特定位置的翻译【交互提示按钮文字】
    //---------------------------------------------------------------------------
    public string m_InteractiveInfo => "交互".GetContent(nameof(GameEvent));
    //---------------------------------------------------------------------------
    //---------------------------------------------------------------------------

    /// <summary>
    /// 使用物品按钮文字
    /// </summary>
    //---------------------------------------------------------------------------
    //public string m_UseItemInfo = "使用物品";
    //---------------------------------------------------------------------------
    //特定位置的翻译【使用物品按钮文字】
    //---------------------------------------------------------------------------
    public string m_UseItemInfo => "使用物品".GetContent(nameof(GameEvent));
    //---------------------------------------------------------------------------
    //---------------------------------------------------------------------------

    /// <summary>
    /// 交互物体的最小距离
    /// </summary>
    const float EVENT_TRIGGER_DISTANCE = 4;

    GameEventManager evtManager
    {
        get
        {
            if(_evtManager == null)
            {
                _evtManager = FindObjectOfType<GameEventManager>();
            }
            return _evtManager;
        }
    }

    GameEventManager _evtManager;


    public void Init()
    {
        //如果有可交互事件，并且有绑定可交互物体。把这些物体设置为交互对象
        if(m_EventType.Contains("1") || m_EventType.Contains("2") ||m_EventType.Contains("3") ||m_EventType.Contains("4"))
        {
            if(m_EventTargets != null && m_EventTargets.Length > 0)
            {
                foreach(var obj in m_EventTargets)
                {
                    if(obj != null && obj.GetComponent<InteractiveObj>() == null)
                    {
                        var interactiveObject = obj.AddComponent<InteractiveObj>();
                        interactiveObject.SetMouseClickCallback(OnClickTarget);
                    }
                }
            }
        }
        else //否则清空该物体的可交互属性
        {
            foreach (var obj in m_EventTargets)
            {
                if (obj == null) continue;
                var o = obj.GetComponent<InteractiveObj>();
                if(o != null)
                {
                    GameObject.Destroy(o);
                }
            }
        }
    }

    void OnClickTarget(InteractiveObj target)
    {
        //BY CGGG 2021/6/9，已经修改为面朝向射线触发，不会再需要鼠标点击
        //DO NOTHING
        return;
    }
    
    void OnTriggerEnter(Collider other)
    {
        var player = Jyx2Player.GetPlayer();
        if (LevelMaster.Instance == null || LevelMaster.Instance.IsInited == false || player == null || this.m_EventType == null )
            return;
        evtManager.OnTriggerEvent(this);
    }

    public async UniTask MarkChest()    
    {
        foreach (var target in m_EventTargets)
        {
            if (target == null) continue;
            var chest = target.GetComponent<MapChest>();
            if (chest != null)
            {
				//使用物品事件为-1时可以直接打开。>0时候需要对应钥匙才能解开。-2时不能打开，参考南贤居宝箱一开始不能打开，交谈后可以直接打开 todo 
                chest.ChangeLockStatus(m_EventType.Contains("3"));
                await chest.MarkAsOpened();
            }
        }
    }


    public static GameEvent GetCurrentSceneEvent(string id)
    {
        var evtGameObj = GameObject.Find("Level/Triggers/" + id);
        if (evtGameObj == null)
            return null;

        return evtGameObj.GetComponent<GameEvent>();
    }

}
