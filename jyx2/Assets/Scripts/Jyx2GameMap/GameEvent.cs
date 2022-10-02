

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameEvent : MonoBehaviour
{
    /// 交互对象
    public GameObject[] m_EventTargets;
    
    /// 直接触发的eventId用m_InteractiveEventId  观察与偷袭是通用的
    [Header("事件类型(0:直接触发；交互,观察,使用物品,偷袭)有哪些写哪些")]
    public String m_EventType = "-1";
    
    /// 交谈事件id
    public String m_InteractiveEventId = "-1";
    
    /// 使用物品事件id (灵活-可下毒，赠送各种效果)
    public String m_UseItemEventId = "-1";
    
    /// 偷袭事件id
    public String m_HitEventId = "-1";
    
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
    
    //unity方法，组件挂载的物体被碰触的事件
    void OnTriggerEnter(Collider other)
    {
        var player = Player.GetPlayer();
        if (LevelMaster.Instance == null || LevelMaster.Instance.IsInited == false || player == null || this.m_EventType == null )
            return;
        evtManager.TryTrigger(this);
    }

    public async UniTask MarkChest()    
    {
        foreach (var target in m_EventTargets)
        {
            if (target == null) continue;
            var chest = target.GetComponent<MapChest>();
            if (chest != null)
            {
				//宝箱物体的使用物品事件为-1时可以直接打开。为n时需要对应钥匙n才能解开。-2时不能打开，参考南贤居宝箱一开始不能打开，交谈后可以直接打开 todo 
                chest.ChangeLockStatus(m_UseItemEventId == "-1");
            }
        }
    }
    
}
