
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cinemachine;
using Configs;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using XLua;
using UnityEngine.Playables;
using Sirenix.Utilities;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;
using Jyx2.Middleware;

namespace Jyx2
{
    public class EventContext
    {
        public int currentItemId;
        
        public static EventContext current = null;
    }
    
    /// lua的桥接函数
    /// 注意：桥接函数都不是运行在Unity主线程中
    [LuaCallCSharp]
    public static class LuaBridge
    {
        static StoryEngine storyEngine { get { return StoryEngine.Instance; } }

        static Semaphore sema = new Semaphore(0, 1);

        static GameRuntimeData runtime { get { return GameRuntimeData.Instance; } }

        public static void Talk(int roleId, string content, string talkName, int type)
        { 
            async void Run()
            {
                storyEngine.BlockPlayerControl = true;
                await UIManager.Instance.ShowUIAsync(nameof(ChatUIPanel), ChatType.RoleId, roleId, content, type, new Action(() =>
                {
                    storyEngine.BlockPlayerControl = false;
                    Next();
                }));
            }

            RunInMainThread(Run);
            
            Wait();
        }
        

        /// <summary>
        /// 修改事件
        /// </summary>
        /// <param name="scene">场景，-2为当前场景</param>
        /// <param name="eventId">事件ID，-2为保留</param>
        /// <param name="canPass">是否能够经过，-2为保留，在本作中没有作用</param>
        /// <param name="changeToEventId">修改为的编号，-2为保留，在本作中没有作用</param>
        /// <param name="interactiveEventId">交互事件ID</param>
        /// <param name="useItemEventId">使用道具事件ID</param>
        /// <param name="enterEventId">进入事件ID</param>
        /// <param name="p7">开始贴图</param>
        /// <param name="p8">结束贴图</param>
        /// <param name="p9">起始贴图</param>
        /// <param name="p10">动画延迟</param>
        /// <param name="p11">X坐标</param>
        /// <param name="p12">Y坐标</param>
        public static void ModifyEvent(
            int scene,
            int eventId,
            int canPass,
            int changeToEventId,
            int interactiveEventId,
            int useItemEventId,
            int enterEventId,
            int p7, int p8, int p9, int p10, int p11, int p12)
        {
            RunInMainThread(() =>
            {
                
                bool isCurrentScene = false;
                //场景ID
                if (scene == -2) //当前场景
                {
                    scene = LevelMaster.GetCurrentGameMap().Id;
                    isCurrentScene = true;
                }

                var evt = GameEvent.GetCurrentGameEvent();
                //事件ID
                if (eventId == -2)
                {
                    if (evt == null)
                    {
                        Debug.LogError("内部错误：当前的eventId为空，但是指定修改当前event");
                        Next();
                        return;
                    }

                    eventId = int.Parse(evt.name); //当前事件
                }
                else
                {
                    evt = GameEventManager.GetGameEventByID(eventId.ToString());
                }

                if (isCurrentScene && evt != null) //当前场景事件如何获取
                {
                    if (interactiveEventId == -2)
                    {
                        interactiveEventId = evt.m_InteractiveEventId;
                    }

                    if (useItemEventId == -2)
                    {
                        useItemEventId = evt.m_UseItemEventId;
                    }

                    if (enterEventId == -2)
                    {
                        enterEventId = evt.m_HitEventId;
                    }
                }
                // 非当前场景事件如何获取
                else
                {
                    if (interactiveEventId == -2)
                    {
                        interactiveEventId = -1;
                    }

                    if (useItemEventId == -2)
                    {
                        useItemEventId = -1;
                    }

                    if (enterEventId == -2)
                    {
                        enterEventId = -1;
                    }
                }

                //更新全局记录
                runtime.ModifyEvent(scene, eventId, interactiveEventId, useItemEventId, enterEventId);

                if (p7 != -2)
                {
                    runtime.SetMapPic(scene, eventId, p7);
                }

                //刷新当前场景中的事件
                LevelMaster.Instance.RefreshGameEvents();
                if (interactiveEventId == -1 && evt != null)
                {
                    async UniTask ExecuteCurEvent()
                    {
                        await evt.MarkChest();
                    }

                    ExecuteCurEvent().Forget();
                }

                //下一条指令
                Next();
            });

            Wait();
        }

        //做选择
        public static int doChoice(string selectMessage,String[] options)
        {
            async void Action()
            {
                List<string> selectionContent = new List<string>(){};
                foreach (var ops in options)
                {
                    selectionContent.Add(ops);
                }
                storyEngine.BlockPlayerControl = true;
                await UIManager.Instance.ShowUIAsync(nameof(ChatUIPanel), ChatType.Selection, "0", selectMessage, selectionContent, new Action<int>((index) =>
                {
                    _selectResult = index;
                    storyEngine.BlockPlayerControl = false;
                    Next();
                }));
            }

            RunInMainThread(Action);

            Wait();
            return _selectResult;
        }
        
        private static bool _battleResult = false;
        
        //开始一场战斗
        public static bool TryBattle(int battleId)
        {
            bool isWin = false;
            RunInMainThread(() => {
                
                //记录当前地图和位置
                ConfigMap currentMap = LevelMaster.GetCurrentGameMap();
                var pos = LevelMaster.Instance.GetPlayerPosition();
                var rotate = LevelMaster.Instance.GetPlayerOrientation();
                
                LevelLoader.LoadBattle(battleId, (ret) =>
                {
                    LevelLoader.LoadGameMap(currentMap, new LevelMaster.LevelLoadPara()
                    {
                        //还原当前地图和位置
                        loadType = LevelMaster.LevelLoadPara.LevelLoadType.ReturnFromBattle,
                        Pos = pos,
                        Rotate = rotate,
                    }, () =>
                    {
                        isWin = (ret == BattleResult.Win);
                        Next();
                    });
                });
            });
            Wait();
            return isWin;
        }
        

        //角色加入，同时获得对方身上的物品
        public static void Join(int roleId)
        {
            RunInMainThread(() => {
                
                if (runtime.JoinRoleToTeam(roleId, true))
                {
                    RoleInstance role = runtime.AllRoles[roleId];
                    storyEngine.DisplayPopInfo(role.Name + "加入队伍！");
                }
                
                Next();
            });
            Wait();
        }
        
        public static void Dead()
        {
            //防止死亡后传送到enterTrigger再次触发事件。临时处理办法
            ModifyEvent(-2, -2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1);

            async void Run()
            {
                await UIManager.Instance.ShowUIAsync(nameof(GameOver));
            }

            RunInMainThread(Run);
        }

        public static bool HaveItem(String itemId,int quality = 0)
        {
            return runtime.Player.GetItem(itemId,quality,false) != null;
        }

        //当前正在使用的物品ID
        public static bool UseItem(int itemId)
        {
            if (EventContext.current == null)
                return false;

            return itemId == EventContext.current.currentItemId;
        }

        //离队
        public static void Leave(int roleId)
        {
            RunInMainThread(() => {

                if (runtime.LeaveTeam(roleId))
                {
                    RoleInstance role = runtime.AllRoles[roleId];
                    storyEngine.DisplayPopInfo(role.Name + "离队。");
                }
                
                Next();
            });
            Wait();
        }

        public static void ZeroAllMP()
        {
            RunInMainThread(() => {
                foreach (var r in runtime.GetTeam())
                {
                    r.Mp = 0;
                }
                Next();
            });
            Wait();
        }

        //修改这个接口逻辑为在当前trigger对应事件序号基础上加上v1,v2,v3 (只对大于0的进行相加，-2保留原事件序号，-1为直接设置)
        // modified by eaphone at 2021/6/12
        public static void Add3EventNum(int scene, int eventId,int v1,int v2,int v3)
        {
            RunInMainThread(() =>
            {
                bool isCurrentScene=false;
                //场景ID
                if (scene == -2) //当前场景
                {
                    scene = LevelMaster.GetCurrentGameMap().Id;
                    isCurrentScene=true;
                }

                var evt=GameEvent.GetCurrentGameEvent();
                //事件ID
                if (eventId == -2)
                {
                    if (evt == null)
                    {
                        Debug.LogError("内部错误：当前的eventId为空，但是指定修改当前event");
                        Next();
                        return;
                    }
                    eventId = int.Parse(evt.name); //当前事件
                }else{
                    evt=GameEventManager.GetGameEventByID(eventId.ToString());
                }

                if(isCurrentScene && evt!=null)//非当前场景事件如何获取
                {
                    if(v1==-2){//值为-2时，取当前值
                        v1=evt.m_InteractiveEventId;
                    }else if(v1>-1){
                        v1+=evt.m_InteractiveEventId;
                    }
                    if(v2==-2){
                        v2=evt.m_UseItemEventId;
                    }else if(v2>-1){
                        v2+=evt.m_UseItemEventId;
                    }
                    if(v3==-2){
                        v3=evt.m_HitEventId;
                    }else if(v3>-1){
                        v3+=evt.m_HitEventId;
                    }
                    
                    runtime.ModifyEvent(scene, eventId, v1, v2, v3);

                    //刷新当前场景中的事件
                    LevelMaster.Instance.RefreshGameEvents();
                }else{
                    if(v1>0){
                        runtime.AddEventCount(scene,eventId,0,v1);
                    }
                    if(v2>0){
                        runtime.AddEventCount(scene,eventId,1,v2);
                    }
                    if(v3>0){
                        runtime.AddEventCount(scene,eventId,2,v3);
                    }
                }

                //下一条指令
                Next();
            });
            Wait();
        }
        
        //targetEvent:0-interactiveEvent, 1-useItemEvent, 2-enterEvent
        public static int jyx2_CheckEventCount(int scene, int eventId, int targetEvent)
        {
            int result=0;
            RunInMainThread(() =>
            {
                //场景ID
                if (scene == -2) //当前场景
                {
                    scene = LevelMaster.GetCurrentGameMap().Id;
                }

                //事件ID
                if (eventId == -2)
                {
                    var evt=GameEvent.GetCurrentGameEvent();
                    if (evt == null)
                    {
                        Debug.LogError("内部错误：当前的eventId为空，但是指定修改当前event");
                        Next();
                        return;
                    }
                    eventId = int.Parse(evt.name); //当前事件
                }
                
                result= runtime.GetEventCount(scene,eventId,targetEvent);
                Next();
            });
            Wait();
            return result;
        }

        public static bool InTeam(int roleId)
        {
            return runtime.GetRoleInTeam(roleId) != null;
        }

        // modify the logicc, when count>=6, team is full
        // by eaphone at 2021/6/5
        public static bool TeamIsFull()
        {
            return runtime.TeamId.Count > GameConst.MAX_TEAMCOUNT - 1;
        }

        public static bool JudgeAttack(int roleId,int low,int high)
        {
            bool ret = JudgeRoleValue(roleId, (r) => {
                int originAttack = r.Attack - r.GetEquipmentProperty("Attack",0) - r.GetEquipmentProperty("Attack",1);

                return originAttack >= low && originAttack <= high;
            });
            return ret;
        }
        
        public static void LearnMagic2(int roleId,int magicId,int noDisplay)
        {
            RunInMainThread(() => {
                var role = runtime.GetRoleInTeam(roleId);

                if (role == null)
                {
                    role = runtime.AllRoles[roleId];
                }

                if (role == null)
                {
                    Debug.LogError("调用了不存在的角色,roleId =" + roleId);
                    Next();
                    return;
                }

                role.LearnMagic(magicId);

                //只有设置了显示，并且角色在队伍的时候才显示
                if(noDisplay != 0 && runtime.TeamId.Contains(roleId))
                {
                    var skill = GameConfigDatabase.Instance.Get<ConfigSkill>(magicId);
                    storyEngine.DisplayPopInfo(role.Name + "习得武学" + skill.Name);
                }
                Next();
            });
            Wait();
        }
        
        public static void SetOneMagic(int roleId,int magicIndexRole,int magicId, int level)
        {
            RunInMainThread(() =>
            {
                var role = runtime.GetRoleInTeam(roleId);

                if (role == null)
                {
                    role = runtime.AllRoles[roleId];
                }

                if (role == null)
                {
                    Debug.LogError("调用了不存在的角色,roleId =" + roleId);
                    Next();
                    return;
                }

                if(magicIndexRole >= role.skills.Count)
                {
                    Debug.LogError("SetOneMagic调用错误，index越界");
                    Next();
                    return;
                }

                role.skills[magicIndexRole].Key = magicId;
                role.skills[magicIndexRole].Level = level;
                Next();
            });
            Wait();
        }
        
        //生命
        public static void AddHp(int roleId, int value)
        {
            RunInMainThread(() =>
            {
                var r = runtime.AllRoles[roleId];
                var v0 = r.MaxHp;
                r.MaxHp = Tools.Limit(v0 + value, 0, GameConst.MAX_HPMP);
                r.Hp = Tools.Limit(r.Hp + value, 0, GameConst.MAX_HPMP);
                storyEngine.DisplayPopInfo(r.Name + "生命增加" + (r.MaxHp - v0));
                Next();
            });
            Wait();
        }
        
        public static void ShowEthics()
        {
            RunInMainThread(() => {
                MessageBox.Create("你现在的品德指数为" + runtime.Player.Moral, Next);
            });
            Wait();
        }

        public static bool JudgeEventNum(int eventIndex, int value)
        {
            bool result = false;
            RunInMainThread(() => {
                var evt = GameEvent.GetCurrentSceneEvent(eventIndex.ToString());
                if(evt != null)
                {
                    result = (evt.m_InteractiveEventId == value);
                }
                Next();
            });
            Wait();
            return result;
        }

        //打开所有场景
        public static void OpenAllScene()
        {
            foreach(var map in GameConfigDatabase.Instance.GetAll<ConfigMap>())
            {
                runtime.SetSceneEntraceCondition(map.Id, 0);
            }
            runtime.SetSceneEntraceCondition(2, 2); //云鹤崖 需要轻功大于75
            runtime.SetSceneEntraceCondition(38, 2); //摩天崖 需要轻功大于75
            runtime.SetSceneEntraceCondition(75, 1); //桃花岛
            runtime.SetSceneEntraceCondition(80, 1); //绝情谷底
        }
        
        //判断场景贴图。ModifyEvent里如果p7!=-2时，会更新对应{场景}_{事件}的贴图信息，可以用此方法JudegeScenePic检查对应的贴图信息
        public static bool JudgeScenePic(int scene, int eventId, int pic)
        {
            bool result = false;
            RunInMainThread(() => {
                //场景ID
                if(scene == -2) //当前场景
                {
                    scene = LevelMaster.GetCurrentGameMap().Id;
                }

                //事件ID
                if(eventId == -2)
                {
                    var evt = GameEvent.GetCurrentGameEvent();
                    if (evt != null)
                    {
                        eventId = int.Parse(evt.name); //当前事件
                    }
                }
                var _target=runtime.GetMapPic(scene, eventId);
                //Debug.LogError(_target);
                result = _target==pic;
                Next();
            });
            Wait();
            return result;
        }
        
        public static void PlayMusic(int id)
        {
            RunInMainThread(() =>
            {
                AudioManager.PlayMusic(id);
            });
        }

        //标记一个场景是否可以进入
        public static void OpenScene(int sceneId)
        {
            runtime.SetSceneEntraceCondition(sceneId, 0);
        }

        // modify by eaphone at 2021/6/5
        public static void SetRoleFace(int dir)
        {
            RunInMainThread(() =>
            {
                var levelMaster = GameObject.FindObjectOfType<LevelMaster>();
                levelMaster.SetRotation(dir);
                Next();
            });
            Wait();
        }

        /// 为角色添加非装备
        public static void NPCGetItem(int roleId,int itemId,int count)
        {
            var role = runtime.AllRoles[roleId];
            if (role != null) role.AlterItem(itemId.ToString(), count);
        }

        public static void PlayWave(int waveIndex)
        {
            RunInMainThread(()=>
            {
                string path = "Assets/BuildSource/sound/e" + (waveIndex < 10 ? ("0" + waveIndex.ToString()) : waveIndex.ToString()) + ".wav";
                AudioManager.PlayClipAtPoint(path, Camera.main.transform.position).Forget();
            });
        }
        
        public static void DarkScence()
        {
            RunInMainThread(() =>
            {
                var blackCover = LevelMaster.Instance.transform.Find("UI/BlackCover");
                if (blackCover == null)
                {
                    Debug.LogError("DarkScence error，找不到LevelMaster/UI/BlackCover");
                    Next();
                    return;
                }

                blackCover.gameObject.SetActive(true);
                var img = blackCover.GetComponent<Image>();
                img.DOFade(1, 1).OnComplete(Next);
            });
            Wait();
        }
        
        
        public static void LightScence()
        {
            RunInMainThread(() =>
            {
                var blackCover = LevelMaster.Instance.transform.Find("UI/BlackCover");
                if (blackCover == null)
                {
                    Debug.LogError("DarkScence error，找不到LevelMaster/UI/BlackCover");
                    Next();
                    return;
                }

                var img = blackCover.GetComponent<Image>();
                img.DOFade(0, 1).OnComplete(() =>
                {
                    blackCover.gameObject.SetActive(false);
                    Next();
                });
            });
            Wait();
        }
        
        public static bool JudgeMoney(int money)
        {
            return (runtime.GetItemCount(GameConst.MONEY_ID) >= money);
        }
        
        /// 添加（减少）物品，并显示提示
        /// <param name="count">可为负数</param>
        public static void AddItem(int itemId, int count,int quality = 0,Boolean isHint = false)
        {
            RunInMainThread(() =>
            {
                var item = runtime.Player.GetItem(itemId.ToString(),quality); 
                runtime.Player.AlterItem(itemId.ToString(),count,item.Quality);
                
                if (isHint)
                {
                    if (count < 0)
                    {
                        storyEngine.DisplayPopInfo("失去物品:".GetContent(nameof(LuaBridge)) + item.Name + "×" + Math.Abs(count));
                    }
                    else
                    {
                        storyEngine.DisplayPopInfo("得到物品:".GetContent(nameof(LuaBridge)) + item.Name + "×" + Math.Abs(count));
                    }
                }
                
            });
        }
        
        //韦小宝商店
        public static void WeiShop()
        {
            async void Action()
            {
                if (LevelMaster.Instance.IsInWorldMap)
                {
                    storyEngine.DisplayPopInfo("大地图中无法打开商店，需到客栈中使用");
                    Next();
                    return;
                }

                string mapId = LevelMaster.GetCurrentGameMap().Id.ToString();
                var hasData = GameConfigDatabase.Instance.Has<ConfigShop>(mapId); // mapId和shopId对应
                if (!hasData)
                {
                    storyEngine.DisplayPopInfo($"地图{mapId}没有配置商店，可在excel/JYX2小宝商店.xlsx中查看");
                    Next();
                    return;
                }

                await UIManager.Instance.ShowUIAsync(nameof(ShopUIPanel), "", new Action(() => { Next(); }));
            }

            RunInMainThread(Action);
            Wait();
        }

        #region 扩展函数
        public static void jyx2_ReplaceSceneObject(string scene,string path, string replace)
        {
            RunInMainThread(() =>
            {
                LevelMasterBooster level = GameObject.FindObjectOfType<LevelMasterBooster>();
                level.ReplaceSceneObject(scene, path, replace);
                Next();
            });
            Wait();
        }
        
        
        // add to handle indoor transport object
        // path: name of destination transform
        // parent: parent path of destination transform
        // target: "" mean transport player. otherwise, need the full path of transport object.
        // eahphone at 2021/6/5
        public static void jyx2_MovePlayer(string path, string parent="Level/Triggers", string target="")
        {
            RunInMainThread(() =>
            {
                var levelMaster = GameObject.FindObjectOfType<LevelMaster>();
                levelMaster.TransportToTransform(parent, path, target);
                Next();
            });
            Wait();
        }

        //主角path= Level/Player
        public static void jyx2_CameraFollow(string path)
        {
            RunInMainThread(() =>
            {
                var followObj = GameObject.Find(path);
                if (followObj == null)
                {
                    Debug.LogError("jyx2_CameraFollow 找不到物体,path=" + path);
                    Next();
                    return;
                }
                var cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
                if (cameraBrain != null)
                {
                    var vcam = cameraBrain.ActiveVirtualCamera;
                    if (vcam != null)
                    {
                        vcam.Follow = followObj.transform;
                    }
                }

                Next();
            });
            Wait();
        }

        //fromName:-1, 获取主角当前位置作为起始点
        public static void jyx2_WalkFromTo(int fromName, int toName) 
        {
            RunInMainThread(() =>
            {
                var fromObj = GameObject.Find("Level/Player");
                if(fromName!=-1){
                    fromObj=GameObject.Find($"Level/NavigateObjs/{fromName}");
                }
                var toObj = GameObject.Find($"Level/NavigateObjs/{toName}");
                if (fromObj == null || toObj == null) 
                {
                    GameUtil.LogError("jyx2_CameraFollow 找不到navigate物体,name=" + fromName + "/" + toName);
                    Next();
                    return;
                }
                LevelMaster.Instance.PlayerWarkFromTo(fromObj.transform.position,toObj.transform.position, () =>
                {
                    Next();
                });
            });
            Wait();
        }

        /// <param name="playableDirector"></param>
        private static void TimeLineNext(PlayableDirector playableDirector)
        {
            Next();
        }

        enum TimeLinePlayMode
        {
            ExecuteNextEventOnPlaying = 0,
            ExecuteNextEventOnEnd = 1,
        }

        static Animator clonePlayer;

        private static float _timelineSpeed = 1;
        
        /// 简单模式播放timeline，播放完毕后直接关闭
        public static void jyx2_PlayTimelineSimple(string timelineName, bool hidePlayer = false)
        {
            RunInMainThread(() =>
            {
                var timeLineRoot = GameObject.Find("Timeline");
                var timeLineObj = timeLineRoot.transform.Find(timelineName);

                if (hidePlayer)
                {
                    var player = LevelMaster.Instance.GetPlayer();
                    player.gameObject.SetActive(false);
                }
                
                if (timeLineObj == null)
                {
                    Debug.LogError("jyx2_PlayTimeline 找不到Timeline,path=" + timelineName);
                    Next();
                    return;
                }
                timeLineObj.gameObject.SetActive(true);
                
                var playableDirector = timeLineObj.GetComponent<PlayableDirector>();
                GameUtil.CallWithDelay(playableDirector.duration, () =>
                {
                    timeLineObj.gameObject.SetActive(false);
                    if (hidePlayer)
                    {
                        var player = LevelMaster.Instance.GetPlayer();
                        player.gameObject.SetActive(true);
                    }
                });
            });
        }
        
        public static void jyx2_PlayTimeline(string timelineName, int playMode, bool isClonePlayer, string tagRole = "")
        {
            RunInMainThread(() =>
            {
                var timeLineRoot = GameObject.Find("Timeline");
                var timeLineObj = timeLineRoot.transform.Find(timelineName);

                if (timeLineObj == null)
                {
                    Debug.LogError("jyx2_PlayTimeline 找不到Timeline,path=" + timelineName);
                    Next();
                    return;
                }

                timeLineObj.gameObject.SetActive(true);
                var playableDirector = timeLineObj.GetComponent<PlayableDirector>();

                if(playMode == (int)TimeLinePlayMode.ExecuteNextEventOnEnd)
                {
                    playableDirector.stopped += TimeLineNext;
                }
                else if (playMode == (int)TimeLinePlayMode.ExecuteNextEventOnPlaying)
                {
                    Next();
                }
                
                playableDirector.Play();

                //timeline播放速度
                if (_timelineSpeed != 1 && _timelineSpeed > 0)
                {
                    playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);    
                }
                
                //UI隐藏
                var mainCanvas = GameObject.Find("MainCanvas");
                if(mainCanvas != null )
                    mainCanvas.transform.Find("MainUI").gameObject.SetActive(false);
                

                //没有指定对象，则默认为主角播放
                if (string.IsNullOrEmpty(tagRole) || tagRole == "PLAYER")
                {
                    //克隆主角来播放特殊剧情
                    if (isClonePlayer)
                    {
                        if (clonePlayer == null)
                        {
                            clonePlayer = GameObject.Instantiate(GameRuntimeData.Instance.Player.View.GetAnimator());
                            clonePlayer.runtimeAnimatorController = null;
                            GameRuntimeData.Instance.Player.View.gameObject.SetActive(false);
                        }

                        DoPlayTimeline(playableDirector, clonePlayer.gameObject);
                    }
                    //正常绑定当前主角播放
                    else
                    {
                        var bindingDic = playableDirector.playableAsset.outputs;
                        bindingDic.ForEach(delegate (PlayableBinding playableBinding)
                        {
                            if (playableBinding.outputTargetType == typeof(Animator))
                            {
                                playableDirector.SetGenericBinding(playableBinding.sourceObject, GameRuntimeData.Instance.Player.View.GetAnimator().gameObject);
                            }
                        });
                    }
                }
                else
                {
                    string objPath = "Level/" + tagRole;
                    GameObject obj = GameObject.Find(objPath);
                    DoPlayTimeline(playableDirector, obj.gameObject);
                }
                
                LevelMaster.Instance.SetPlayerCanController(false);
            });
            Wait();
        }

        static void DoPlayTimeline(PlayableDirector playableDirector, GameObject player)
        {
            player.SetActive(false);

            var bindingDic = playableDirector.playableAsset.outputs;
            bindingDic.ForEach(delegate (PlayableBinding playableBinding)
            {
                if (playableBinding.outputTargetType == typeof(Animator))
                {
                    if (playableBinding.sourceObject != null)
                    {
                        playableDirector.GetComponent<PlayableDirectorHelper>().BindPlayer(player);
                    }
                    playableDirector.SetGenericBinding(playableBinding.sourceObject, player);
                }
            });
        }

        public static void jyx2_StopTimeline(string timelineName)
        {
            RunInMainThread(() =>
            {
                //UI恢复
                var mainCanvas = GameObject.Find("MainCanvas");
                if(mainCanvas != null )
                    mainCanvas.transform.Find("MainUI").gameObject.SetActive(true);
                
                var timeLineRoot = GameObject.Find("Timeline");
                var timeLineObj = timeLineRoot.transform.Find(timelineName);

                if (timeLineObj == null)
                {
                    Debug.LogError("jyx2_PlayTimeline 找不到Timeline,path=" + timelineName);
                    Next();
                    return;
                }

                var playableDiretor = timeLineObj.GetComponent<PlayableDirector>();
                playableDiretor.stopped -= TimeLineNext;
                timeLineObj.gameObject.SetActive(false);

                GameRuntimeData.Instance.Player.View.gameObject.SetActive(true);
                GameRuntimeData.Instance.Player.View.GetAnimator().transform.localPosition = Vector3.zero;
                GameRuntimeData.Instance.Player.View.GetAnimator().transform.localRotation = Quaternion.Euler(Vector3.zero);
                if(clonePlayer != null)
                {
                    GameObject.Destroy(clonePlayer.gameObject);
                }
                clonePlayer = null;

                playableDiretor.GetComponent<PlayableDirectorHelper>().ClearTempObjects();
                LevelMaster.Instance.SetPlayerCanController(true);

                Next();
            });
            Wait();
        }

        public static void jyx2_Wait(float duration)
        {
            RunInMainThread(() =>
            {
                Sequence seq = DOTween.Sequence();
                seq.AppendCallback(Next)
                .SetDelay(duration / _timelineSpeed);
            });
            Wait();
        }
        
        /// 切换角色动作
        /// 调用样例（胡斐居）
        /// jyx2_SwitchRoleAnimation("Level/NPC/胡斐", "Assets/BuildSource/AnimationControllers/打坐.controller")
        public static void jyx2_SwitchRoleAnimation(string rolePath, string animationControllerPath, string scene = "")
        {
            Debug.Log("jyx2_SwitchRoleAnimation called");

            RunInMainThread(() =>
            {
                LevelMasterBooster level = GameObject.FindObjectOfType<LevelMasterBooster>();
                if (level == null)
                {
                    Debug.LogError("jyx2_SwitchRoleAnimation调用错误，找不到LevelMaster");
                    Next();
                    return;
                }

                level.ReplaceNpcAnimatorController(scene, rolePath, animationControllerPath);
                Next();
            });
            Wait();
        }

        public static void jyx2_FixMapObject(string key, string value)
        {
            RunInMainThread(() =>
            {
                runtime.KeyValues[key] = value;
                var objs = GameObject.FindObjectsOfType<FixWithGameRuntime>();
                if (objs != null)
                {
                    foreach(var obj in objs)
                    {
                        if(key==obj.Flag)
                            obj.Reload();
                        else continue;
                    }
                }
                LevelMasterBooster level = GameObject.FindObjectOfType<LevelMasterBooster>();
                level.RefreshSceneObjects();
                Next();
            });

            Wait();
        }
        
        #endregion


        #region private

        private static void RunInMainThread(Action run)
        {
            Loom.QueueOnMainThread(_ =>
            {
                run();
            }, null);
        }
        
        /// 等待返回
        private static void Wait()
        {
            sema.WaitOne();
        }
        
        /// 下一条指令
        private static void Next()
        {
            sema.Release();
        }

        private static int _selectResult;
        
        private static bool JudgeRoleValue(int roleId, Predicate<RoleInstance> judge)
        {
            bool result = false;
            RunInMainThread(() =>
            {
                var role = runtime.GetRoleInTeam(roleId);
                if(role == null)
                {
                    role = runtime.AllRoles[roleId];
                }
                if (role == null)
                {
                    Debug.LogError("调用了不存在的role，roleid=" + roleId);
                    result = false;
                    Next();
                    return;
                }

                result = judge(role);
                Next();
            });

            Wait();
            return result;
        }


        #endregion
    }
}
