
using Jyx2;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// <summary>
/// 统一管理所有的事件触发
/// </summary>
public class GameEventManager : MonoBehaviour
{
    GameEvent curEvent = null;
    const int NO_EVENT = -1;
    
    public bool OnTriggerEvent(GameEvent evt)
    {
        if (evt == curEvent)
            return false;

        //如果已有交互事件进行，并且自己事件不是立刻触发事件，则让一下优先级 
        if ( curEvent !=null && (curEvent.m_EventType.Contains("1") || curEvent.m_EventType.Contains("3") ) && ! evt.m_EventType.Contains("0") )
            return false;

        //关闭之前的事件
        if (curEvent != null && curEvent != evt)
        {
            OnExitEvent(curEvent);
        }

        //设置当前事件
        curEvent = evt;
        return TryTrigger(evt);
    }

    public void OnExitEvent(GameEvent evt)
    {
        if (evt == curEvent)
        {
            curEvent = null;
        }

        UnityTools.DisHighLightObjects(evt.m_EventTargets);
        UIManager.Instance.HideUI(nameof(InteractUIPanel));
    }

    public void OnExitAllEvents()
    {
        if (curEvent == null)
            return;
        
        UnityTools.DisHighLightObjects(curEvent.m_EventTargets);
        UIManager.Instance.HideUI(nameof(InteractUIPanel));
        curEvent = null;
    }
    

    /// <summary>
    /// 显示交互面板
    /// </summary>
    async void ShowInteractUIPanel(GameEvent evt)
    {
        var uiParams = new List<object>();
        int buttonCount = 0;
        
        //交谈
        if (evt.m_EventType.Contains("1"))
        {
            uiParams.Add("交互");
            uiParams.Add(new Action(() =>
            {
                ExecuteJyx2Event(curEvent.m_InteractiveEventId);
            }));
            buttonCount++;
        }
        
        //观察
        if (evt.m_EventType.Contains("2"))
        {
            uiParams.Add("观察");
            uiParams.Add(new Action(() =>
            {
                OnClickedUseItemButton();
            }));
            buttonCount++;
        }

        //使用道具
        if (evt.m_EventType.Contains("3"))
        {
            uiParams.Add("使用物品");
            uiParams.Add(new Action(() =>
            {
                OnClickedUseItemButton();
            }));
            buttonCount++;
        }
        
        //偷袭 todo 改点击事件内容
        if (evt.m_EventType.Contains("4"))
        {
            uiParams.Add("偷袭");
            uiParams.Add(new Action(() =>
            {
                OnClickedUseItemButton();
            }));
            buttonCount++;
        }

        if (buttonCount == 1)
        {
            await UIManager.Instance.ShowUIAsync(nameof(InteractUIPanel), uiParams[0], uiParams[1]);
        }
        else if (buttonCount == 2)
        {
            await UIManager.Instance.ShowUIAsync(nameof(InteractUIPanel), uiParams[0], uiParams[1], uiParams[2], uiParams[3]);
        }
        else if (buttonCount == 3)
        {
            await UIManager.Instance.ShowUIAsync(nameof(InteractUIPanel), uiParams[0], uiParams[1], uiParams[2], uiParams[3], uiParams[4], uiParams[5]);
        }
        else if (buttonCount == 4)
        {
            await UIManager.Instance.ShowUIAsync(nameof(InteractUIPanel), uiParams[0], uiParams[1], uiParams[2], uiParams[3], uiParams[4], uiParams[5], uiParams[6], uiParams[7]);
        }
    }

    Button GetUseItemButton()
    {
        var root = GameObject.Find("LevelMaster/UI");
        var btn = root.transform.Find("UseItemButton").GetComponent<Button>();
        return btn;
    }


    async void OnClickedUseItemButton()
    {
        await UIManager.Instance.ShowUIAsync(nameof(BagUIPanel), GameRuntimeData.Instance.Player.Items, new Action<String>((itemId) =>
        {
            if (itemId == null) //取消使用
                return;
            //使用道具
            ExecuteJyx2Event(curEvent.m_UseItemEventId, new JYX2EventContext() { currentItemId = int.Parse(itemId) });
        }),null);
    }
    
    bool TryTrigger(GameEvent evt)
    {
        //直接触发
        if (evt.m_EventType.Contains("0") && !LuaExecutor.isExcutling())
        {
            ExecuteJyx2Event(evt.m_InteractiveEventId);
            return true;
        }

        //事件类型填写格式判断
        if (!evt.m_EventType.Contains("1") && !evt.m_EventType.Contains("2") && !evt.m_EventType.Contains("3") 
            && !evt.m_EventType.Contains("4")) return false;
        if (evt.m_EventTargets == null || evt.m_EventTargets.Length == 0) return false;

        //显示交互面板
        ShowInteractUIPanel(evt);

        UnityTools.HighLightObjects(evt.m_EventTargets, Color.red);

        return true;
    }

    public void ExecuteJyx2Event(int eventId, JYX2EventContext context = null)
    {
        if (eventId < 0)
        {
            //Debug.LogError("执行错误的luaEvent，id=" + eventId);
            return;
        }

        //停止导航
        var levelMaster = LevelMaster.Instance;

        //fix player stop moving after interaction UI confirm
        if (levelMaster != null && eventId != 911)
        {
            // fix drag motion continuous move the player when scene is playing
            // modified by eaphone at 2021/05/31
            levelMaster.SetPlayerCanController(false);
            levelMaster.StopPlayerNavigation();
        }
        
        SetCurrentGameEvent(curEvent);
        
        //设置运行环境上下文
        JYX2EventContext.current = context;

        async UniTask ExecuteCurEvent()
        {
            //if (curEvent != null)
            //    await curEvent.MarkChest();
            
            //先判断是否有蓝图类
            //如果有则执行蓝图，否则执行lua
            var graph = await Jyx2ResourceHelper.LoadEventGraph(eventId);
            if (graph != null)
            {
                graph.Run(OnFinishEvent);
            }
            else
            {
                var eventLuaPath = "jygame/ka" + eventId;
                Jyx2.LuaExecutor.Execute(eventLuaPath, OnFinishEvent);
            }
        }

        ExecuteCurEvent().Forget();
    }

    void OnFinishEvent()
    {
        JYX2EventContext.current = null;

        SetCurrentGameEvent(null);
        // fix drag motion continuous move the player when scene is playing
        // modified by eaphone at 2021/05/31
        var levelMaster = LevelMaster.Instance;
        if (levelMaster != null)
        {
            levelMaster.SetPlayerCanController(true);
        }

        if (curEvent != null)
        {
            UnityTools.DisHighLightObjects(curEvent.m_EventTargets);
        }

        curEvent = null;
    }

    static string _currentEvt;
    public static void SetCurrentGameEvent(GameEvent evt)
    {
        if (evt == null)
        {
            _currentEvt = "";
        }
        else
        {
            _currentEvt = evt.name;
        }
    }
    public static GameEvent GetCurrentGameEvent()
    {
        return GetGameEventByID(_currentEvt);
    }
	
	public static GameEvent GetGameEventByID(string id)
	{
        if (string.IsNullOrEmpty(id))
            return null;

        foreach (var evt in FindObjectsOfType<GameEvent>())
        {
            if (evt.name == id)
                return evt;
        }

        return null;
	}
}
