
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

        public static void Talk(String roleId, string content, string talker)
        { 
            async void Run()
            {
                storyEngine.BlockPlayerControl = true;
                await UIManager.Instance.ShowUIAsync(nameof(ChatUIPanel), ChatType.RoleId, roleId, content, talker, new Action(() =>
                {
                    storyEngine.BlockPlayerControl = false;
                    Next();
                }));
            }

            RunInMainThread(Run);
            
            Wait();
        }
        
        /// 修改事件
        /// <param name="scene">场景id，this为当前场景</param>
        /// <param name="eventObject">事件触发点NPC或物体名，this为当前物体</param>
        /// <param name="interactiveEventId">交互事件ID(0为不变更)</param>
        /// <param name="useItemEventId">使用道具事件ID(0为不变更)</param>
        /// <param name="enterEventId">直接触发事件ID(0为不变更)</param>
        public static void ModifyEvent(
            String scene,
            String eventObject,
            String interactiveEventId,
            String useItemEventId,
            String enterEventId)
        {
            RunInMainThread(() =>
            {
                //场景ID
                if (scene == "this") //当前场景
                {
                    scene = LevelMaster.GetCurrentGameMap().Id.ToString();
                }

                var curEvent = GameEventManager.GetGameEventByID(GameEventManager._currentEvt);//当前场景 当前触发的事件
                //事件ID
                if (eventObject == "this")
                {
                    eventObject = curEvent.name; //当前碰触的物体
                }
                else
                {
                    curEvent = GameEventManager.GetGameEventByID(eventObject.ToString());
                }

                //更新全局记录
                runtime.ModifyEvent(scene, eventObject, interactiveEventId, useItemEventId, enterEventId);
                
                //刷新当前场景中的事件
                LevelMaster.Instance.RefreshGameEvents();
                if (interactiveEventId == "-1" && curEvent != null)
                {
                    async UniTask ExecuteCurEvent()
                    {
                        await curEvent.MarkChest();
                    }

                    ExecuteCurEvent().Forget();
                }

                //下一条指令
                Next();
            });

            Wait();
        }

        //做选择
        public static int doChoice(string selectMessage,String[] options,String roleId,String talker = "")
        {
            async void Action()
            {
                List<string> selectionContent = new List<string>(){};
                foreach (var ops in options)
                {
                    selectionContent.Add(ops);
                }
                storyEngine.BlockPlayerControl = true;
                await UIManager.Instance.ShowUIAsync(nameof(ChatUIPanel), ChatType.Selection, roleId, selectMessage, selectionContent, talker, new Action<int>((index) =>
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
            //ModifyEvent(-2, -2, -1, -1, -1);

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
        
        //学习一个技能，或使技能升一级
        public static void LearnMagic(int roleId,int magicId,int noDisplay,int level = 1)
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
                var item = GameConfigDatabase.Instance.Get<ConfigItem>(itemId); //从配置表取物品名称
                runtime.Player.AlterItem(itemId.ToString(),count,quality);
                
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
                level.SetSceneInfo(path, replace, scene);
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
        /// SwitchRoleAnimation("Level/NPC/胡斐", "Assets/BuildSource/AnimationControllers/打坐.controller")
        public static void SwitchRoleAnimation(string rolePath, string animationControllerPath, string scene = "")
        {
            Debug.Log("SwitchRoleAnimation called");

            RunInMainThread(() =>
            {
                LevelMasterBooster level = GameObject.FindObjectOfType<LevelMasterBooster>();
                if (level == null)
                {
                    Debug.LogError("SwitchRoleAnimation调用错误，找不到LevelMaster");
                    Next();
                    return;
                }

                level.SetSceneInfo(rolePath, "controller:" + animationControllerPath, scene);
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
