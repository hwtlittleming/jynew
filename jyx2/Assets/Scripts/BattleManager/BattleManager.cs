

using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;
using Jyx2;

using Jyx2.Battle;
using Jyx2.Middleware;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
    
    public Boolean isPause = false; //是否暂停 策略中会暂停

    public List<BattleBlockData> block_list = new List<BattleBlockData>(); //战场上的格子总列表
    
    private static BattleManager _instance;

    #region 战场组件

    private BattleFieldModel m_BattleModel;

    public BattleFieldModel GetModel()
    {
        return m_BattleModel;
    }
    
    //设置主角
    public RoleInstance _player
    {
        get { return Teammates.Values.First(); }
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
    public Dictionary<String,RoleInstance> Teammates;

    //敌人
    public Dictionary<String,RoleInstance> Enermys;

    public async UniTask StartBattle(Dictionary<String,RoleInstance> enermyRoleList,Dictionary<String,RoleInstance> ourRoleList,Action<BattleResult> callback)
    {
        Debug.Log("-----StartBattle----");

        Teammates = ourRoleList;
        Enermys = enermyRoleList;
        
        if (IsInBattle) return;
        var tempView = _player.View;
        
        IsInBattle = true;
        //初始化战斗model
        m_BattleModel = new BattleFieldModel();
        //初始化范围逻辑
        //rangeLogic = new RangeLogic(BattleboxHelper.Instance.IsBlockExists, m_BattleModel.BlockHasRole);
        
        //await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        await UniTask.WaitForEndOfFrame();

        //地图上所有单位进入战斗,加入战场，待命动画，面向对面
        foreach (var role in enermyRoleList.Values)
        {
            AddBattleRole(role); 

            role.View.LazyInitAnimator();
            
            //默认选中的技能初始化
            if (role.CurrentSkill >= role.skills.Count)
            {
                role.CurrentSkill = 0;
            }
            role._currentSkill = role.skills[role.CurrentSkill];
            role.SwitchAnimationToSkill(role._currentSkill, true);

        }
        foreach (var role in ourRoleList.Values)
        {
            AddBattleRole(role); 

            role.View.LazyInitAnimator();
            
            //默认选中的技能初始化
            if (role.CurrentSkill >= role.skills.Count)
            {
                role.CurrentSkill = 0;
            }
            role._currentSkill = role.skills[role.CurrentSkill];
            role.SwitchAnimationToSkill(role._currentSkill, true);
           
        }
        
        //战斗开始提示
        await UIManager.Instance.ShowUIAsync(nameof(CommonTipsUIPanel), TipsType.MiddleTop, "战斗开始".GetContent(nameof(BattleManager))); //提示UI
        //展示角色血条
        await UIManager.Instance.ShowUIAsync(nameof(BattleMainUIPanel), BattleMainUIState.ShowHUD); 
        
        //battleloop
        var model = GetModel();

        enermyRoleList.Values.ToList().Sort((x, y) => { return x.CompareTo(y); } ); //排序 未测
        //添加脚本 开始攻击
        addScript(Teammates);
        addScript(enermyRoleList);
        
        
    }
    public async void addScript(Dictionary<String,RoleInstance> roleDic ){
        for(int i = 0; i< roleDic.Count;i++)
        {
            var transform = GameObject.Find(roleDic.ElementAt(i).Key).transform;
            transform.gameObject.AddComponent<BattleUnit>();
            //给格子上的战斗单元脚本初始化属性
            transform.gameObject.GetComponent<BattleUnit>()._role = roleDic.ElementAt(i).Value;
            transform.gameObject.GetComponent<BattleUnit>()._manager = this;
            transform.gameObject.GetComponent<BattleUnit>().trans = transform;
            await UniTask.Delay(1000); //速度快的先一秒动手
        }
    }

    //AI行动
    public async void planAndAttack(RoleInstance _role,BattleUnit b,int actPoints)
    {
        return;
        _role.View.Say("就你叫小白？",2000);
        _role.View.ShowAttackInfo($"<color=green>中毒</color>");//飘字 改大
        //如何体现出学习带来的强度:某一招多次使用对其威力下降或闪避提升(天赋效果 ，我们怎么决策有优势他跟着学习，我们怎么决策给我方带来额外利益 他对抗之(吸蓝，提升自己某防御，封禁某法术，降低普攻)镜像boss
        var motivation = _role.currentMotivation;
        motivation += _role.motivationPerSecond;
        
        //获取AI计算结果 AI攻击方式 普攻 技能 投掷 使用物品
        await AIManager.Instance.GetAIResult(_role);
        await UniTask.Delay(_role.Speed); //普攻等待 和技能等待时间不同
        b.isCd = false;
        //再执行具体逻辑
        //await ExecuteAIResult(_role, aiResult);
        BattleBlockData bd = block_list.Find((blockData) => blockData.x == 3 && blockData.y == 2 && blockData.team == "we");
        //await AttackOnce(_role, _role.GetZhaoshis(false).FirstOrDefault(), bd);
    }

    public async void operate(RoleInstance _role,BattleUnit b,int actPoints)
    {

            UniTaskCompletionSource<ManualResult> t = new UniTaskCompletionSource<ManualResult>();
            
            Action<ManualResult> callback = delegate(ManualResult result) { t.TrySetResult(result); };
            
            //显示战斗选项面板，等待返回选择的策略; 因异步加载战斗选项UI后 要等点击了某个技能再继续执行所以用到了UniTaskCompletionSource
            await UIManager.Instance.ShowUIAsync(nameof(BattleActionUIPanel),_role, callback);
            
            //等待完成
            await t.Task;
            
            //关闭面板
            UIManager.Instance.HideUI(nameof(BattleActionUIPanel));
            
            //返回
            ManualResult ret = t.GetResult(0);
            
            if (ret.choose == "normalAttack")
            {
                String Weapon = _role.Equipments[0] == null ? null : _role.Equipments[0].Name;
                SkillInstance skill = new SkillInstance();
                if (Weapon == null)
                {
                    skill = new SkillInstance(0);//物攻-拳击
                }else if (Weapon.Contains("剑"))
                {
                    skill = new SkillInstance(1);//物攻-挥剑
                }else if (Weapon.Contains("刀"))
                {
                    skill = new SkillInstance(2);//物攻-挥刀
                }else if (Weapon.Contains("杖"))
                {
                    skill = new SkillInstance(3);//物攻-挥杖
                }else if (Weapon.Contains("弓"))
                {
                    skill = new SkillInstance(4);//物攻-射击
                }else if (Weapon.Contains("枪"))
                {
                    skill = new SkillInstance(5);//物攻-射击
                }
               
                //普攻间隔后改为通过配置普攻技能时长控制
                //await UniTask.Delay(_role.NormalAttackSpeed); 
                await AttackOnce(_role, _role.skills.FirstOrDefault(), ret.BlockData); //todo 普攻动作的耗时要配短
                

            }else if (ret.choose == "skillAttack")
            {
                await AttackOnce(_role, ret.Skill, ret.BlockData);
                //攻击间隔等待时间 
                //await UniTask.Delay(_role.Speed);
            }else if (ret.choose == "defend")
            {
                await AttackOnce(_role, ret.Skill, ret.BlockData);
                //攻击间隔等待时间 
                //await UniTask.Delay(_role.Speed);
            }else if (ret.choose == "useItem")
            {
                //使用道具
               // await RoleUseItem(role, aiResult.Item,role);
            }

            b.isCd = false;

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
        UIManager.Instance.HideUI(nameof(BattleMainUIPanel));

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
    
    /// 添加角色到战场里面
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
        /*if (role.View.m_IsKeyRole && role.IsDead())
        {
            role.Stun(-1);
        }*/
    }

    string CalExpGot(ConfigBattle battleData)
    {
        List<RoleInstance> alive_teammate = m_BattleModel.Teammates;
        List<RoleInstance> dead_teammates = m_BattleModel.Dead.Where(r => r.team == 0).ToList();
        List<RoleInstance> teammates = alive_teammate.Union(dead_teammates).ToList();
        string rst = "";
        foreach (var role in alive_teammate)
        {
            int expAdd = 0;//battleData.Exp / alive_teammate.Count();
            role.ExpGot += expAdd;
        }

        foreach (var role in teammates)
        {
            if (role.ExpGot > 0)
                rst += string.Format("{0}获得战斗经验{1}\n".GetContent(nameof(BattleManager)), role.Name, role.ExpGot);
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
    
    
        //普攻，伤害技能，治疗技能
        public async UniTask AttackOnce(RoleInstance role, SkillInstance skill,BattleBlockData blockData)
        {
            //Debug.Log(role.Name + "使用" + skill.Data.Name + "攻击" + blockData.blockName);
            if (role == null || skill == null || blockData == null)
            {
                GameUtil.LogError("AttackOnce入参为空");
                return;
            }
            
            //检测攻击距离合法性 AI不校验
            if (role.team == 0 && skill.ToWhichSide == 0 && blockData.blockName.Contains("they"))
            {
                GameUtil.DisplayPopinfo("释放位置错误");
                return;
            }
            if (role.team == 0 && skill.ToWhichSide == 1 )
            {
                if (blockData.blockName.Contains("we"))
                {
                    GameUtil.DisplayPopinfo("释放位置错误");return;
                }
                if (blockData.y > role.bestAttackDistance)
                {
                    GameUtil.DisplayPopinfo("攻击距离不够");return;
                }
            }

            
            
            //测试技能编辑器添加
           GameRuntimeData.Instance.Player = new RoleInstance(0);
            skill.Display = GameConfigDatabase.Instance.Get<ConfigSkill>(skill.ConfigId).Display;
            //skill =new SkillInstance(skill.ConfigId);

            List<RoleInstance> beHitRoleList = new List<RoleInstance>();
            
            role.View.LookAtBattleBlock(blockData.WorldPos); //先面向目标
            role.SwitchAnimationToSkill(skill); //切换姿势

            //获取攻击范围
            List<BattleBlockData> blockList = new List<BattleBlockData>(); //攻击涵盖的格子
            List<Transform> blockTransList = new List<Transform>(); //攻击涵盖的格子位置集合
            List<RoleInstance> toRoleList = new List<RoleInstance>(); //攻击涵盖的角色
            if (skill.Name == "普通攻击")
            {
                /*String attackRange = role.Equipments[0].attackRange.ToString(); //攻击范围，默认为空:点攻击，名武器才有值
                if (attackRange != null && attackRange != "0")
                {
                    // todo 名武器攻击格子    剑1 竖排 剑2横排；刀1 撇捺 杖1:上下左右 
                }*/
                
                /*ItemInstance weapon = role.Equipments[0];
                int dis = weapon.bestDistance; *///获得持有武器的最佳攻击距离
                
                blockList.Add(blockData);
                blockTransList.Add(blockData.blockObject.transform);
            }
            else
            {
                var covertype = skill.SkillCoverType;
                
                blockList.Add(blockData);
                blockTransList.Add(blockData.blockObject.transform);
            }

            //获取覆盖格子上的人
            foreach (var b in blockList)
            {
                Enermys.TryGetValue(b.blockName, out RoleInstance toRole);
                if (role.team == 1)
                {
                    Teammates.TryGetValue(b.blockName, out  toRole);
                }
                toRoleList.Add(toRole);
                
                var result = AIManager.Instance.GetSkillResult(role, toRole, skill); 

                result.Run();

                //当需要播放受攻击动画时，不直接刷新血条，延后到播放受攻击动画时再刷新。其他情况直接刷新血条。
                if (result.damage > 0 || result.damageMp > 0)
                {
                    //加入到受击动作List
                    beHitRoleList.Add(toRole);
                }
                else
                {
                    toRole.View.MarkHpBarIsDirty();
                }
            }
            
            SkillCastHelper castHelper = new SkillCastHelper
            {
                Source = role.View,
                CoverBlocks = blockTransList,
                skill = skill,
                Targets = beHitRoleList.ToMapRoles(),
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
            public SkillInstance Skill = null;
            //public AIResult aiResult = null;
        }
        
        public BattleBlockData GetBlockData(int x,int y,String team)
        {
            return block_list.Find(bd => bd.x == x && bd.y == y && (bd.team == team || bd.team == "public"));
        }
}