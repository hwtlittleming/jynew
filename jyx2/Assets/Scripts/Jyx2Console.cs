
using System.Linq;
using Configs;
using Jyx2;
using UnityEngine;

public static class Jyx2Console
{
    public static void RunConsoleCommand(string cmd)
    {
        if (string.IsNullOrEmpty(cmd))
            return;

        string[] paras = cmd.Split(' ');
        switch (paras[0].ToLower())
        {
            case "map":
            case "scene":
            {
                int id = int.Parse(paras[1]);
                var map = ConfigMap.Get(id);
                if (map != null)
                {
                    LevelLoader.LoadGameMap(map);
                }

                break;
            }
            case "event":
            {
                string eventId = paras[1];
                var eventManager = GameObject.FindObjectOfType<GameEventManager>();
                eventManager.ExecuteEvent(eventId);
                break;
            }
            case "item":
            {
                int itemId = int.Parse(paras[1]);
                int count = 1;
                if (paras.Length > 2)
                {
                    count = int.Parse(paras[2]);
                }

                LuaBridge.AddItem(itemId, count);
                break;
            }
            case "set_attack":
            {
                int attack = int.Parse(paras[1]);
                GameRuntimeData.Instance.Player.Attack = attack;
                break;
            }
            //开启无敌
            case "whosyourdad":
            {
                if (paras.Length > 1)
                {
                    BattleManager.Whosyourdad = int.Parse(paras[1]) == 1;
                }
                else
                {
                    BattleManager.Whosyourdad = true;
                }

                break;
            }
            case "transportwei":
            {
                TransportWei();
                break;
            }
            default:
                Debug.Log("没有识别的指令，将执行lua替代：" + cmd);
                string luaContent = cmd;
                Jyx2.LuaExecutor.ExecuteLuaAsync(luaContent);
                break;
        }
    }


    // transport Wei to other hotel when leave hotel if had talked to him
    // added by eaphone at 2021/6/5
    public static void TransportWei()
    {
        var weiPath = "Dynamic/韦小宝";
        var triggerPath = "Level/Triggers";
        var cur = LevelMaster.GetCurrentGameMap();
        var isWeiAtCurMap = GameObject.Find(weiPath);
        var isTalkedToWei = false;
        if (isWeiAtCurMap != null && isWeiAtCurMap.activeSelf)
        {
            var hotelList = GameConfigDatabase.Instance.GetAll<ConfigShop>().ToList();
            LevelMasterBooster level = GameObject.FindObjectOfType<LevelMasterBooster>();
            var ran = new System.Random();
            var index = ran.Next(0, hotelList.Count);
            while (cur.Id == hotelList[index].Id)
            {
                index = ran.Next(0, hotelList.Count);
            }

            GameObject eventsParent = GameObject.Find("Level/Triggers");
            foreach (Transform t in eventsParent.transform)
            {
                var evt = t.GetComponent<GameEvent>();
                if (evt == null) continue;
                foreach (var obj in evt.m_EventTargets)
                {
                    if (obj == null) continue;
                    isTalkedToWei = evt.m_InteractiveEventId == "938";
                    /*var o = obj.GetComponent<InteractiveObj>();
                    if (o != null && "韦小宝" == o.name)
                    {
                        isTalkedToWei = evt.m_InteractiveEventId == 938;

                    }*/
                }
            }

            if (isTalkedToWei)
            {
                var curTriggerId = GameConfigDatabase.Instance.Get<ConfigShop>(cur.Id).Trigger.ToString();
                Debug.Log("transport Wei to " + hotelList[index].Id);
                level.SetSceneInfo(weiPath, "0", cur.Id.ToString());
                level.SetSceneInfo(weiPath, "1", hotelList[index].Id.ToString());
                GameRuntimeData.Instance.ModifyEvent(cur.Id.ToString(), curTriggerId, "-1", "-1", "-1");
                GameRuntimeData.Instance.ModifyEvent(hotelList[index].Id.ToString(), hotelList[index].Trigger.ToString(), "938", "-1",
                    "-1");
                LevelMaster.Instance.RefreshGameEvents();
            }
        }
    }
    
}