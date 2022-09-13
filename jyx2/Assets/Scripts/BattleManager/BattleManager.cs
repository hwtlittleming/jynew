

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using i18n.TranslatorDef;
using Jyx2;

using Jyx2.Battle;
using Jyx2.Middleware;
using Jyx2Configs;
using UnityEngine;
using Random = UnityEngine.Random;


public class BattleStartParams
{
    public Action<BattleResult> callback; //战斗结果
    public List<RoleInstance> roles; //参与战斗的角色
    public ConfigBattle battleData; //战斗地图数据
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("BattleManager");
                go.hideFlags = HideFlags.HideInHierarchy;
                DontDestroyOnLoad(go);
                _instance = GameUtil.GetOrAddComponent<BattleManager>(go.transform);
            }

            return _instance;
        }
    }

    public List<BattleBlockData> block_list = new List<BattleBlockData>(); //战场上的格子总列表
    
    private static BattleManager _instance;

    #region 战场组件

    private BattleFieldModel m_BattleModel;

    public BattleFieldModel GetModel()
    {
        return m_BattleModel;
    }


    /*private RangeLogic rangeLogic;

    public RangeLogic GetRangeLogic()
    {
        return rangeLogic;
    }*/

    private RoleInstance _player
    {
        get { return GameRuntimeData.Instance.Player; }
    }

    #endregion

    //是否无敌
    public static bool Whosyourdad = false;


    public bool IsInBattle = false;
    private BattleStartParams m_battleParams;
    private AudioClip lastAudioClip;
    
    public List<RoleInstance> roles; //参与战斗的角色
    public GameObject m_roleFocusRing;

    //队友
    public List<RoleInstance> Teammates;

    //敌人
    public List<RoleInstance> Enermys;

    public async UniTask StartBattle(List<RoleInstance> enermyRoleList,List<RoleInstance> ourRoleList,Action<BattleResult> callback)
    {
        Debug.Log("StartBattle called");

        Teammates = ourRoleList;
        Enermys = enermyRoleList;
        
        if (IsInBattle) return;
        var tempView = _player.View;
        if (tempView == null)
        {
            tempView = ourRoleList[0].View;
        }

        IsInBattle = true;
        //初始化战斗model
        m_BattleModel = new BattleFieldModel();
        //初始化范围逻辑
        //rangeLogic = new RangeLogic(BattleboxHelper.Instance.IsBlockExists, m_BattleModel.BlockHasRole);
        
        //await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        await UniTask.WaitForEndOfFrame();

        //地图上所有单位进入战斗,加入战场，待命动画，面向对面
        foreach (var role in enermyRoleList)
        {
            role.ResetBattleSkill();
            role.EnterBattle();
            AddBattleRole(role);
        }
        
        foreach (var role in ourRoleList)
        {
            role.ResetBattleSkill();
            role.EnterBattle();
            AddBattleRole(role);
        }

        m_BattleModel.InitBattleModel(); //战场初始化 行动顺序排序这些
        //---------------------------------------------------------------------------
        //await Jyx2_UIManager.Instance.ShowUIAsync(nameof(CommonTipsUIPanel), TipsType.MiddleTop, "战斗开始"); //提示UI
        //---------------------------------------------------------------------------
        //特定位置的翻译【战斗开始时候的弹窗提示】
        //---------------------------------------------------------------------------
        await Jyx2_UIManager.Instance.ShowUIAsync(nameof(CommonTipsUIPanel), TipsType.MiddleTop, "战斗开始".GetContent(nameof(BattleManager))); //提示UI
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------

        await Jyx2_UIManager.Instance.ShowUIAsync(nameof(BattleMainUIPanel), BattleMainUIState.ShowHUD); //展示角色血条
        
        //battleloop
        var model = GetModel();

        //敌人开始自由攻击
        foreach (var r in enermyRoleList)
        {
            var transform = GameObject.Find(r.blockData.blockName).transform;
            transform.gameObject.AddComponent<BattleUnit>();
            //给格子上的战斗单元脚本初始化属性
            transform.gameObject.GetComponent<BattleUnit>()._role = r;
            transform.gameObject.GetComponent<BattleUnit>()._manager = this;
        }
        
        foreach (var r in Teammates)
        {
            var transform = GameObject.Find(r.blockData.blockName).transform;
            transform.gameObject.AddComponent<BattleUnit>();
            //给格子上的战斗单元脚本初始化属性
            transform.gameObject.GetComponent<BattleUnit>()._role = r;
            transform.gameObject.GetComponent<BattleUnit>()._manager = this;

            await UniTask.Delay(1000);
        }

    }
    public async void planAndAttack(RoleInstance _role,BattleUnit b)
    {
        
        //如何体现出学习带来的强度:某一招多次使用对其威力下降或闪避提升(天赋效果 魔瓶滚动天赋UI)，我们怎么决策有优势他跟着学习，我们怎么决策给我方带来额外利益 他对抗之(吸蓝，提升自己某防御，封禁某法术，降低普攻)镜像boss
        var motivation = _role.currentMotivation;
        motivation += _role.motivationPerSecond;
        
        //获取AI计算结果 AI攻击方式 普攻 技能 投掷 使用物品
        await AIManager.Instance.GetAIResult(_role);
        await UniTask.Delay(_role.Speed);
        b.isActing = false;
        //再执行具体逻辑
        //await ExecuteAIResult(_role, aiResult);
        BattleBlockData bd = block_list.Find((blockData) => blockData.x == 3 && blockData.y == 2 && blockData.team == "we");
        //await AttackOnce(_role, _role.GetZhaoshis(false).FirstOrDefault(), bd);
    }

    public async void operate(RoleInstance _role)
    {
        
        while (true)
        {
            UniTaskCompletionSource<ManualResult> t = new UniTaskCompletionSource<ManualResult>();
            
            Action<ManualResult> callback = delegate(ManualResult result) { t.TrySetResult(result); };
            
            //显示战斗选项面板，等待返回选择的策略; 因异步加载战斗选项UI后 要等点击了某个技能再继续执行所以用到了UniTaskCompletionSource
            await Jyx2_UIManager.Instance.ShowUIAsync(nameof(BattleActionUIPanel),_role, callback);
            
            //等待完成
            await t.Task;
            
            //关闭面板
            Jyx2_UIManager.Instance.HideUI(nameof(BattleActionUIPanel));
            
            //返回
            ManualResult ret = t.GetResult(0);
            
            if (ret.choose == "normalAttack")
            {
                await AttackOnce(_role, _role.GetZhaoshis(false).FirstOrDefault(), ret.BlockData);
            }else if (ret.choose == "skillAttack")
            {
                await AttackOnce(_role, ret.Skill, ret.BlockData);
            }
        }
        /*if (ret.isRevert) //点击取消
        {
            isSelectMove = true;
            role.movedStep = 0;
            role.Pos = originalPos;
        }
        else if (ret.movePos != null && isSelectMove) //移动
        {
            isSelectMove = false;
            role.movedStep += originalPos.GetDistance(ret.movePos);
            await RoleMove(role, ret.movePos);
        }else if (ret.isWait) //等待
        {
            _manager.GetModel().ActWait(role);
            break;
        }else if (ret.isAuto) //托管给AI
        {
            role.Pos = originalPos;
            await RoleAIAction(role);
            break;
        }
        else if (ret.aiResult != null) //具体执行行动逻辑（攻击、道具、用毒、医疗等）
        {
            role.movedStep = 0;
            await ExecuteAIResult(role, ret.aiResult);
            break;
        }*/

    }

    public void OnBattleEnd(BattleResult result)
    {
        switch (result)
        {
            case BattleResult.Win:
            {
                string bonusText = CalExpGot(m_battleParams.battleData);
                GameUtil.ShowFullSuggest(bonusText, "<color=yellow><size=50>战斗胜利</size></color>".GetContent(nameof(BattleManager)), delegate
                {
                    EndBattle();
                    m_battleParams.callback?.Invoke(result);
                    m_battleParams = null;
                });
                break;
            }
            case BattleResult.Lose:
            {
                
                GameUtil.ShowFullSuggest("胜败乃兵家常事，请大侠重新来过。".GetContent(nameof(BattleManager)), "<color=red><size=80>战斗失败！</size></color>".GetContent(nameof(BattleManager)), delegate
                {
                    EndBattle();
                    m_battleParams.callback?.Invoke(result);
                    //if (m_battleParams.backToBigMap) //由dead指令实现返回主界面逻辑
                    //    LevelLoader.LoadGameMap("Level_BigMap");
                    m_battleParams = null;
                });
                break;
            }
        }
        
        //所有人至少有1HP
        foreach (var role in GameRuntimeData.Instance.GetTeam())
        {
            if (role.Hp <= 0)
                role.Hp = 1;
        }
    }

    //清扫战场
    public void EndBattle()
    {
        IsInBattle = false;
        Jyx2_UIManager.Instance.HideUI(nameof(BattleMainUIPanel));

        //临时，需要调整
        foreach (var role in m_BattleModel.Roles)
        {
            //role.LeaveBattle();
            //非KeyRole死亡2秒后尸体消失
            if (role.IsDead() && role.View != null && role.View.gameObject != null && !role.View.m_IsKeyRole)
            {
                role.View.gameObject.SetActive(false);
            }
        }

        //rangeLogic = null;
        m_BattleModel.Roles.Clear();
    }


    /// <summary>
    /// 添加角色到战场里面
    /// </summary>
    /// <param name="role"></param>
    /// <param name="team"></param>
    public void AddBattleRole(RoleInstance role)
    {
        int team = role.team;
        //加入战场
        m_BattleModel.AddBattleRole(role, role.Pos, team, (team != 0));
        
        //待命
        role.View.Idle();
        
        //面向对面 暂且这样
        var otherside = new Vector3(role.View.transform.position.x + 3,role.View.transform.position.y,role.View.transform.position.z);
        if (team == 1) otherside = new Vector3(role.View.transform.position.x - 3,role.View.transform.position.y,role.View.transform.position.z);
        role.View.LookAtWorldPosInBattle(otherside);

        //死亡的关键角色默认晕眩
        if (role.View.m_IsKeyRole && role.IsDead())
        {
            role.Stun(-1);
        }
    }

    string CalExpGot(ConfigBattle battleData)
    {
        List<RoleInstance> alive_teammate = m_BattleModel.Teammates;
        List<RoleInstance> dead_teammates = m_BattleModel.Dead.Where(r => r.team == 0).ToList();
        List<RoleInstance> teammates = alive_teammate.Union(dead_teammates).ToList();
        string rst = "";
        foreach (var role in alive_teammate)
        {
            int expAdd = battleData.Exp / alive_teammate.Count();
            role.ExpGot += expAdd;
        }

        foreach (var role in teammates)
        {
            if (role.ExpGot > 0)
                //---------------------------------------------------------------------------
                //rst += string.Format("{0}获得经验{1}\n", role.Name, role.ExpGot);
                //---------------------------------------------------------------------------
                //特定位置的翻译【战斗胜利角色获得经验的提示】
                //---------------------------------------------------------------------------
                rst += string.Format("{0}获得经验{1}\n".GetContent(nameof(BattleManager)), role.Name, role.ExpGot);
                //---------------------------------------------------------------------------
                //---------------------------------------------------------------------------

                role.Exp += role.ExpGot;

            //避免越界
            role.Exp = Tools.Limit(role.Exp, 0, GameConst.MAX_EXP);
      

            //根据资质增长属性
            Boolean t = role.CanLevelUp();
        }

        return rst;
    }


    #region 战斗共有方法
    

    #endregion
    
     //中毒受伤
     async UniTask RunPosionHurtLogic(RoleInstance role)
        {
            int hurtEffect = role.Hurt / 20;

            int hurtEffectRst = Tools.Limit(hurtEffect, 0, role.Hp);

            if (hurtEffect == 0 ) return;

            if (hurtEffectRst > 0)
            {
                role.View?.ShowAttackInfo($"<color=white>-{hurtEffectRst}</color>");
                role.Hp -= hurtEffectRst;
            }

            if (role.Hp < 1)
                role.Hp = 1;

            //只有实际中毒和受伤才等待
            role.View?.MarkHpBarIsDirty();
            await UniTask.Delay(TimeSpan.FromSeconds(0.8));
        }

        //AI角色行动
        async UniTask RoleAIAction(RoleInstance role)
        {
            //获取AI计算结果
            await AIManager.Instance.GetAIResult(role);
            
            //先移动
            //await RoleMove(role, new BattleBlockVector(aiResult.MoveX, aiResult.MoveY));

            //再执行具体逻辑
            //await ExecuteAIResult(role, aiResult);
        }

        //执行具体逻辑
        async UniTask ExecuteAIResult(RoleInstance role, AIResult aiResult)
        {
            if (aiResult.Item != null)
            {
                //使用道具
                await RoleUseItem(role, aiResult.Item,role);
            }
            else if (aiResult.Zhaoshi != null)
            {
                //使用技能
                //await RoleCastSkill(role, aiResult.Zhaoshi, new BattleBlockVector(aiResult.AttackX, aiResult.AttackY));
            }
            else
            {
                //休息
                role.OnRest();
            }
        }

        //角色移动
        /*async UniTask RoleMove(RoleInstance role, BattleBlockVector moveTo)
        {
            if (role == null || moveTo == null)
            {
                GameUtil.LogError("enter move state failed");
                return;
            }

            //寻找移动路径
            var path = FindMovePath(role, moveTo);
            if (path == null || path.Count == 0)
                return;

            //播放奔跑动画
            role.View.Run();

            //播放移动
            await role.View.transform.DOPath(path.ToArray(), path.Count * 0.2f).SetLookAt(0).SetEase(Ease.Linear);

            //idle动画
            role.View.Idle();

            //设置逻辑位置
            role.Pos = moveTo;
            /*var enemy = AIManager.Instance.GetNearestEnemy(role);
            if (enemy != null)
            {
                //面向最近的敌人
                role.View.LookAtWorldPosInBattle(enemy.View.transform.position);
            }#1#
        }*/


        //做一次施展伤害技能或普攻,普攻也视为一次特殊skill；blockData:攻击选择点所在格子信息
        public async UniTask AttackOnce(RoleInstance role, BattleZhaoshiInstance skill,BattleBlockData blockData)
        {
            Debug.Log(role.Name + "使用" + skill.Data.Name + "攻击" + blockData.blockName);
            if (role == null || skill == null || blockData == null)
            {
                GameUtil.LogError("AttaclOncel失败");
                return;
            }
            //todo blockData 格子上要加人物 role,实例化格子游戏对象BattleBlockData 格子的事务
            List<RoleInstance> beHitAnimationList = new List<RoleInstance>();
            
            role.View.LookAtBattleBlock(blockData.WorldPos); //先面向目标
            role.SwitchAnimationToSkill(skill.Data); //切换姿势

            //获取攻击范围
            List<BattleBlockData> blockList = new List<BattleBlockData>(); //攻击涵盖的格子
            List<Transform> blockTransList = new List<Transform>(); //攻击涵盖的格子位置集合
            List<RoleInstance> toRoleList = new List<RoleInstance>(); //攻击涵盖的角色, todo 角色属性变化 格子上的角色属性是否能跟着变化
            if (skill.Data.Name == "普通攻击")
            {
                /*String attackRange = role.Equipments[0].attackRange.ToString(); //攻击范围，默认为空:点攻击，名武器才有值
                if (attackRange != null && attackRange != "0")
                {
                    // todo 名武器攻击格子 横1 竖2 
                }*/
                
                ItemInstance weapon = role.Equipments[0];
                int dis = weapon.bestDistance; //获得持有武器的最佳攻击距离
                
                blockList.Add(blockData);
                blockTransList.Add(blockData.blockObject.transform);
            }
            else
            {
                skill.CastCD(); //技能CD
                skill.CastCost(role);skill.CastMP(role); //技能消耗
                var covertype = skill.Data.SkillCoverType;
                // todo 技能的攻击格子
                blockList.Add(blockData);
                blockTransList.Add(blockData.blockObject.transform);
            }

            foreach (var b in blockList)
            {
                //还活着
                if (b.role == null || b.role.IsDead()) continue;
                toRoleList.Add(b.role);
                
                // todo 伤害的计算
                SkillCastResult rst = new SkillCastResult(role, b.role, skill);
                var result = AIManager.Instance.GetSkillResult(role, b.role, skill); 

                result.Run();

                //当需要播放受攻击动画时，不直接刷新血条，延后到播放受攻击动画时再刷新。其他情况直接刷新血条。
                if (result.IsDamage())
                {
                    //加入到受击动作List
                    beHitAnimationList.Add(b.role);
                }
                else
                {
                    b.role.View.MarkHpBarIsDirty();
                }
            }
            
            SkillCastHelper castHelper = new SkillCastHelper
            {
                Source = role.View,
                CoverBlocks = blockTransList,
                Zhaoshi = skill,
                Targets = beHitAnimationList.ToMapRoles(),
            };

            //攻击和受击动画播放
            await castHelper.Play();
            
        }

        //使用道具 修改后可用
        public async UniTask RoleUseItem(RoleInstance role, ItemInstance item,RoleInstance toRole)
        {
            Debug.Log(role.Name + "使用" + item.Name + "攻击" + toRole.blockData.blockName);
            
            AnimationClip clip = null;
            var itemType = (int)item.ItemType;
            if (itemType == 3)
                clip = GlobalAssetConfig.Instance.useItemClip; //选择吃药的动作
            else if (itemType == 4)
                clip = GlobalAssetConfig.Instance.anqiClip; //选择使用暗器的动作

            //如果配置了动作，则先播放动作
            if (clip != null)
            {
                await role.View.PlayAnimationAsync(clip, 0.25f);
            }

            toRole.UseItem(item);

            if (role.team == 0 && role.Id == 0) //如果是己方角色，则从背包里扣。 队友暂时是扔自己配置了携带的物品，是否要改？
            {
                GameRuntimeData.Instance.Player.AlterItem(item.ConfigId, -1);
            }
            else //否则从角色身上扣
            {
                toRole.AlterItem(item.ConfigId, -1);
            }

            Dictionary<int, int> effects = UIHelper.GetItemEffect(item);
            foreach (var effect in effects)
            {
                if (!GameConst.ProItemDic.ContainsKey(effect.Key.ToString()))
                    continue;
                PropertyItem pro = GameConst.ProItemDic[effect.Key.ToString()];
                if (effect.Key == 15 || effect.Key == 17)
                {
                    toRole.View.ShowBattleText($"{pro.Name}+{effect.Value}", Color.blue);
                }
                else if (effect.Key == 6 || effect.Key == 8 || effect.Key == 26)
                {
                    string valueText = effect.Value > 0 ? $"+{effect.Value}" : effect.Value.ToString();
                    toRole.View.ShowBattleText($"{pro.Name}{valueText}", Color.green);
                }
                else if (effect.Key == 13 || effect.Key == 16)
                {
                    toRole.View.ShowBattleText($"{pro.Name}+{effect.Value}", Color.white);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        }
        
        //手动控制操作结果
        public class ManualResult
        {
            public String choose;
            public BattleBlockData BlockData = null;
            public BattleZhaoshiInstance Skill = null;
            //public AIResult aiResult = null;
        }
        
        public BattleBlockData GetBlockData(int x,int y,String team)
        {
            return block_list.Find(bd => bd.x == x && bd.y == y && bd.team == team);
        }
}