
using Jyx2;

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// 统一管理所有的事件触发
public class GameEventManager : MonoBehaviour
{
    public  static GameEvent curEvent;
    
    public void OnExitAllEvents()
    {
        if (curEvent == null)
            return;
        
        UnityTools.DisHighLightObjects(curEvent.m_EventTargets);
        UIManager.Instance.HideUI(nameof(InteractUIPanel));
        curEvent = null;
    }
    
    /// 显示交互面板
    async void ShowInteractUIPanel(GameEvent evt)
    {
        var uiParams = new List<object>();
        int buttonCount = 0;
        
        //交互
        if (evt.m_EventType.Contains("交互"))
        {
            uiParams.Add("交互");
            uiParams.Add(new Action(() =>
            {
                ExecuteEvent(evt.m_InteractiveEventId);
            }));
            buttonCount++;
        }
        
        //观察
        if (evt.m_EventType.Contains("观察"))
        {
            uiParams.Add("观察");
            uiParams.Add(new Action(() =>
            {
                OnClickedUseItemButton();
            }));
            buttonCount++;
        }

        //使用道具
        if (evt.m_EventType.Contains("使用物品"))
        {
            uiParams.Add("使用物品");
            uiParams.Add(new Action(() =>
            {
                OnClickedUseItemButton();
            }));
            buttonCount++;
        }
        
        //偷袭 todo 改点击事件内容
        if (evt.m_EventType.Contains("偷袭"))
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

    async void OnClickedUseItemButton()
    {
        await UIManager.Instance.ShowUIAsync(nameof(BagUIPanel), GameRuntimeData.Instance.Player.Items, new Action<String>((itemId) =>
        {
            if (itemId == null) //取消使用
                return;
            //使用道具
            ExecuteEvent(curEvent.m_UseItemEventId, new EventContext() { currentItemId = int.Parse(itemId) });
        }),null);
    }
    
    //所有触发入口
    public  bool TryTrigger(GameEvent evt)
    {
        //如果已有事件进行
        if ( curEvent !=null )
            return false;
        
        //设置当前事件
        curEvent = evt;
        
        //直接触发
        if (evt.m_EventType.Contains("0") && !LuaExecutor.isExcutling())
        {
            ExecuteEvent(evt.m_InteractiveEventId);
            return true;
        }

        //事件类型填写格式判断
        if (!evt.m_EventType.Contains("交互") && !evt.m_EventType.Contains("观察") && !evt.m_EventType.Contains("使用物品") 
            && !evt.m_EventType.Contains("偷袭)")) return false;
        if (evt.m_EventTargets == null || evt.m_EventTargets.Length == 0) return false;

        //显示交互面板，选择事件
        ShowInteractUIPanel(evt);

        UnityTools.HighLightObjects(evt.m_EventTargets, Color.red);

        return true;
    }

    //执行eventgraph
    public void ExecuteEvent(String eventId, EventContext context = null)
    {
        if (eventId == "-1") return; 
            
        //停止导航
        var levelMaster = LevelMaster.Instance;

        if (levelMaster != null)
        {
            levelMaster.SetPlayerCanController(false);
            levelMaster.StopPlayerNavigation();
        }
        
        SetCurrentGameEvent(curEvent);
        
        //设置运行环境上下文
        EventContext.current = context;

        async UniTask ExecuteCurEvent()
        {
            //if (curEvent != null)
            //    await curEvent.MarkChest();
            
            //先判断是否有蓝图类，有则执行蓝图，否则执行lua
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
        EventContext.current = null;

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

    public static string _currentEvt;
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
