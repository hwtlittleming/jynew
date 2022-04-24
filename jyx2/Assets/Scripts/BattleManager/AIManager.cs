/*
 * 金庸群侠传3D重制版
 * https://github.com/jynew/jynew
 *
 * 这是本开源项目文件头，所有代码均使用MIT协议。
 * 但游戏内资源和第三方插件、dll等请仔细阅读LICENSE相关授权协议文档。
 *
 * 金庸老先生千古！
 */
using Jyx2;


using Jyx2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Jyx2.Middleware;
using Jyx2Configs;
using Sirenix.Utilities;
using UnityEngine;
using Random = System.Random;

//AI计算相关
public class AIManager
{
    private static AIManager _instance;
    public static AIManager Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = new AIManager();
                _instance.Init();
            }
            return _instance;
        }
    }

    RangeLogic rangeLogic 
    {
        get 
        {
            return BattleManager.Instance.GetRangeLogic();
        }
    }
    BattleManager _battleManager 
    {
        get 
        {
            return BattleManager.Instance;
        }
    }
    
    BattleFieldModel BattleModel 
    {
        get 
        {
            return BattleManager.Instance.GetModel();
        }
    }
    private void Init()
    {
    }

    public async UniTask GetAIResult(RoleInstance role)
    {
        var Enermies = _battleManager.Enermys;
        var Teammates = _battleManager.Teammates;

        if (role.team == 1) //如果是敌方 相对关系转换
        {
            var temp = Enermies;
            Enermies = Teammates;
            Teammates = temp;
        }
        //去除已死目标
        foreach (var e in Enermies)
        {
            if (e.IsDead())
            {
                Enermies.Remove(e);
            }
        }
        foreach (var e in Teammates)
        {
            if (e.IsDead())
            {
                Teammates.Remove(e);
            }
        }

        if (Teammates.IsNullOrEmpty() || Enermies.IsNullOrEmpty())
        {
            var model = _battleManager.GetModel();
            //判断战斗是否结束
            var rst = model.GetBattleResult();
            if (rst != BattleResult.InProgress)
            {
                _battleManager.OnBattleEnd(rst);
                return;
            }
        }

        int iq = role.IQ;
        
        //AI可执行的策略组
        List<String> strategies = new List<String>();
        strategies.Add("normalAttack");//普攻
        strategies.Add("useSkill");//技能
        strategies.Add("useItem");//吃药
        strategies.Add("throw"); //弃剑 或 投掷物品 扔武器是最后一击 高智快死之前会扔武器或自杀式袭击
        
        double maxscore = 0;
        
        //iq 0~30 30~60 >60 >90
        Random r = new Random();
        BattleBlockData toBlockData = Enermies[UnityEngine.Random.Range(0,Enermies.Count-1)].blockData;//随机获取一个存活敌人位置
        BattleBlockData weBlockData = Teammates[UnityEngine.Random.Range(0,Teammates.Count-1)].blockData;//随机获取一个存活队友位置
        IEnumerable<BattleZhaoshiInstance> skills = role.GetZhaoshis(false);//所有技能
        if (skills.Count() == 0)
        {
            Debug.Log(role.Name + "没有技能！");
            return;
        }
        var skill = skills.ElementAt(0);//默认的普攻 配置时要把不耗蓝的普攻放第一个技能，否则默认拿key=0的普攻动作
        if (skill == null)
        {
            skill = new BattleZhaoshiInstance(new SkillInstance(0));
        }
        if (iq < 30)//低智 完全随机 技能使用率 0.2
        {
            if ( r.Next(1, 11) > 8) 
            {
                skill = skills.ElementAt(r.Next(0,skills.Count()));//随机选择一个技能
            }
            //随机选择一个技能攻击(包括普攻和投掷)
            await _battleManager.AttackOnce(role,skill,toBlockData);
            return;
        }
        else if (iq < 60) //初智 低血吃药 不会给对面加血 技能使用率 0.4
        {
            if ((role.Hp < role.Hp * 0.2 || role.Mp < role.Mp * 0.2) && role.Items.Count > 0)
            {
                List<Jyx2ConfigItem> items = GetAvailableItems(role, 3); //获得携带物品
                Jyx2ConfigItem item = items.ElementAt(r.Next(0,items.Count()));
                //使用道具
                await _battleManager.RoleUseItem(role,item,toBlockData.role);
                return;
            }
            if (r.Next(1, 11) > 6)
            {
                skill = skills.ElementAt(r.Next(0, skills.Count())); //随机选择一个技能
                if (skill.IsCastToEnemy() == true)
                {
                    await _battleManager.AttackOnce(role, skill, toBlockData);
                }
                else
                {
                    await _battleManager.AttackOnce(role, skill, weBlockData);
                }
                return;
            }
            else
            {
                await _battleManager.AttackOnce(role, skill, toBlockData);
                return;
            }
        } 
        else //有智商的敌人(一般AI) 技能使用率 0.6 决策增加自爆 吃刚好的药 投掷物品决定于收益最大； 攻击弱者；技能覆盖范围最大; iq 60以上攻击加成
        {
            //优先给队伍吃药
            foreach (var teammate in Teammates)
            {
                if (teammate.Items.Count > 0 && (teammate.Hp < 0.2 * teammate.MaxHp || teammate.Mp < 0.2 * teammate.MaxMp))
                {
                    List<Jyx2ConfigItem> items = GetAvailableItems(role, 3);
                    Jyx2ConfigItem _item = items.FirstOrDefault();
                    foreach (var item in items)
                    {
                        double score = 0;
                        //尽量吃刚刚好的药
                        if (item.AddHp > 0)
                        {
                            score += Mathf.Min(item.AddHp, role.MaxHp - role.Hp) - item.AddHp / 10;
                        }
                        if (item.AddMp > 0)
                        {
                            score += Mathf.Min(item.AddMp, role.MaxMp - role.Mp) / 2 - item.AddMp / 10;
                        }
                        if (score > maxscore)
                        {
                            maxscore = score;
                            _item = item;
                        }
                    }
                    await _battleManager.RoleUseItem(role,_item,teammate);
                    return;
                }
            }
            //伤害技能或普攻  覆盖敌人最多的技能
            if ( r.Next(1, 11) > 4)
            {
                skill = skills.ElementAt(r.Next(0, skills.Count()-1)); //随机选择一个技能
                if (skill.IsCastToEnemy() == true)
                {
                    await _battleManager.AttackOnce(role, skill, toBlockData);
                }
                else
                {
                    await _battleManager.AttackOnce(role, skill, weBlockData);
                }
                return;
            }
            await _battleManager.AttackOnce(role, skill, toBlockData);
            return;
            
            if (iq >= 90) //其中聪明绝顶的敌人 范围治疗和增益 技能使用率0.8 血低会 小怪自爆/boss扔武器 技能偏向解除我方增益，法攻高则禁魔，物攻高提升己方防御等  可选:逃跑让损失经验? todo 学习对抗策略
            {
                
            }
        }
    }
    /// </summary>
    /// 战斗计算公式可以参考：https://tiexuedanxin.net/thread-365140-1-1.html
    ///
    /// 
    /// </summary>
    /// <param name="r1"></param>
    /// <param name="r2"></param>
    /// <param name="skill"></param>
    /// <param name="blockVector"></param>
    /// <returns></returns>
    public SkillCastResult GetSkillResult(RoleInstance r1, RoleInstance r2, BattleZhaoshiInstance skill)
    {        
        SkillCastResult rst = new SkillCastResult(r1, r2, skill);
        var magic = skill.Data.GetSkill();
        int level_index = skill.Data.GetLevel()-1;//此方法返回的是显示的武功等级，1-10。用于calMaxLevelIndexByMP时需要先-1变为数组index再使用
        level_index = skill.calMaxLevelIndexByMP(r1.Mp, level_index)+1;//此处计算是基于武功等级数据index，0-9.用于GetSkillLevelInfo时需要+1，因为用于GetSkillLevelInfo时需要里是基于GetLevel计算的，也就是1-10.
        //普通攻击
        if (magic.DamageType == 0)
        {
            //队伍1武学常识
            int totalWuxue = BattleModel.GetTotalWuXueChangShi(r1.team);

            //队伍2武学常识
            int totalWuxue2 = BattleModel.GetTotalWuXueChangShi(r2.team);

            if (r1.Mp <= magic.MpCost) //已经不够内力释放了
            {
                rst.damage = 1 + UnityEngine.Random.Range(0, 10);
                return rst;
            }
            //总攻击力＝(人物攻击力×3 ＋ 武功当前等级攻击力)/2 ＋武器加攻击力 ＋ 防具加攻击力 ＋ 武器武功配合加攻击力 ＋我方武学常识之和
            int attack = ((r1.Attack - r1.GetWeaponProperty("Attack") - r1.GetArmorProperty("Attack")) * 3 ) / 2 + r1.GetWeaponProperty("Attack") + r1.GetArmorProperty("Attack") + r1.GetExtraAttack(magic) + totalWuxue;
            
            //总防御力 ＝ 人物防御力 ＋武器加防御力 ＋ 防具加防御力 ＋ 敌方武学常识之和
            int defence = r2.Defense + totalWuxue2;

            //伤害 ＝ （总攻击力－总防御×3）×2 / 3 + RND(20) – RND(20)                  （公式1）
            int v = (attack - defence * 3) * 2 / 3 + UnityEngine.Random.Range(0, 20) - UnityEngine.Random.Range(0, 20);
            
            //如果上面的伤害 < 0 则
            //伤害 ＝  总攻击力 / 10 + RND(4) – RND(4)                                            （公式2）
            if (v <= 0)
            {
                v = attack / 10 + UnityEngine.Random.Range(0, 4) - UnityEngine.Random.Range(0, 4);
            }

            //7、如果伤害仍然 < 0 则    伤害 ＝ 0
            if (v <= 0)
            {
                v = 0;
            }
            else
            {
                //8、if  伤害 > 0 then
                //    伤害＝ 伤害 ＋ 我方体力/15  ＋ 敌人受伤点数/ 20
                v = v + r1.Tili / 15 + r2.Hurt / 20;
            }
            
            //点、线、十字的伤害，距离就是两人相差的格子数，最小为1。
            //面攻击时，距离是两人相差的格子数＋敌人到攻击点的距离。
            int dist = 1;
            if (skill.GetCoverType() == SkillCoverType.RECT)
            {
                dist += 1; //blockVector.GetDistance(r2.Pos);
            }

            //9、if 双方距离 <= 10 then
            //    伤害 ＝ 伤害×（100 -  ( 距离–1 ) * 3 ）/ 100
            //else
            //    伤害 ＝ 伤害*2 /3
            if (dist <= 10)
            {
                v = v * (100 - (dist - 1) * 3) / 100;
            }
            else
            {
                v = v * 2 / 3;
            }

            //10、if 伤害 < 1  伤害 ＝ 1
            if (v < 1)
                v = 1;

            rst.damage = v;

            //敌人受伤程度
            rst.hurt = v / 10;

            //攻击带毒
            //中毒程度＝武功等级×武功中毒点数＋人物攻击带毒
            int add = 10 + r1.AttackPoison;
            //if 敌人抗毒> 中毒程度 或 敌人抗毒> 90 则不中毒
            //敌人中毒=敌人已中毒＋ 中毒程度/15
            //if 敌人中毒> 100 then 敌人中毒 ＝100
            //if 敌人中毒<0 then 敌人中毒=0
            if (r2.AntiPoison <= add && r2.AntiPoison <= 90)
            {
                int poison = Tools.Limit(add / 15, 0, GameConst.MAX_ROLE_ATK_POISON);
                rst.poison = poison;
            }
  
            return rst;
        }
        else if ((int)magic.DamageType == 1) //吸内
        {
            /*var levelInfo = skill.Data.GetSkillLevelInfo();
            
            //杀伤内力逻辑
            int v = levelInfo.KillMp;
            v += UnityEngine.Random.Range(0, 3) - UnityEngine.Random.Range(0, 3);
            rst.damageMp = v;

            //吸取内力逻辑
            int addMp = levelInfo.AddMp;
            if (addMp > 0)
            {
                rst.addMaxMp = UnityEngine.Random.Range(0, addMp / 2);
                addMp += UnityEngine.Random.Range(0, 3) - UnityEngine.Random.Range(0, 3);
                rst.addMp = addMp;    
            }*/
            
            return rst;
        }
        else if ((int)magic.DamageType == 2) //用毒 -GameUtil::usePoison
        {
            rst.poison = usePoison(r1, r2);
            return rst;
        }
        else if ((int)magic.DamageType == 3) //解毒
        {
            rst.depoison = detoxification(r1, r2);
            return rst;
        }
        else if ((int)magic.DamageType == 4) //治疗
        {
            var _rst = medicine(r1, r2);
            rst.heal = _rst.heal;
            rst.hurt = _rst.hurt;
            return rst;
        }
        else if ((int)magic.DamageType == 5) //暗器
        {
            var anqi = skill.Anqi;
            var _rst = hiddenWeapon(r1, r2, anqi);
            rst.damage = _rst.damage;
            rst.hurt = _rst.hurt;
            rst.poison = _rst.poison;
            return rst;
        }
        return null;
    }

    List<Jyx2ConfigItem> GetAvailableItems(RoleInstance role, int itemType)
    {
        List<Jyx2ConfigItem> items = new List<Jyx2ConfigItem>();
        foreach (var item in role.Items)
        {
            var tmp = item.Item;
            if ((int)tmp.ItemType == itemType)
                items.Add(tmp);
        }
        return items;
    }


    //用毒
    /// </summary>
    /// 中毒计算公式可以参考：https://tiexuedanxin.net/thread-365140-1-1.html
    /// 也参考War_PoisonHurt：https://github.com/ZhanruiLiang/jinyong-legend
    /// 
    /// </summary>
    /// <param name="r1"></param>
    /// <param name="r2"></param>
    /// <returns></returns>
    int usePoison(RoleInstance r1, RoleInstance r2)
    {
        //中毒程度 ＝（用毒能力－抗毒能力）/ 4
        int poison = (r1.UsePoison - r2.AntiPoison) / 4;
        //小于0则为0
        if (poison < 0)
            poison = 0;
        return poison;
    }

    //医疗
    /// </summary>
    /// 医疗计算公式可以参考：https://tiexuedanxin.net/forum.php?mod=viewthread&tid=394465
    /// 也参考ExecDoctor：https://github.com/ZhanruiLiang/jinyong-legend
    /// 
    /// </summary>
    /// <param name="r1"></param>
    /// <param name="r2"></param>
    /// <returns></returns>
    SkillCastResult medicine(RoleInstance r1, RoleInstance r2)
    {
        SkillCastResult rst = new SkillCastResult();
        if (r2.Hurt > r1.Heal + 20)
        {
            GameUtil.DisplayPopinfo("受伤太重无法医疗");
            return rst;
        }
        //增加生命 = 医疗能力 * a + random(5);
        //当受伤程度 > 75, a = 1 / 2;
        //当50 < 受伤程度 <= 75, a = 2 / 3;
        //当25 < 受伤程度 <= 50, a = 3 / 4;
        //当0 < 受伤程度 <= 25, a = 4 / 5;
        //当受伤程度 = 0，a = 4 / 5;
        int a = (int)Math.Ceiling((double)r2.Hurt / 25);
        if (a == 0) a = 1;
        int addHp = r1.Heal * (5 - a) / (6 - a) + UnityEngine.Random.Range(0, 5);
        rst.heal = addHp;
        //减低受伤程度 = 医疗能力.
        rst.hurt = -addHp;
        return rst;
    }

    //解毒
    /// </summary>
    /// 解毒计算公式可以参考ExecDecPoison：https://github.com/ZhanruiLiang/jinyong-legend
    ///
    /// 
    /// </summary>
    /// <param name="r1"></param>
    /// <param name="r2"></param>
    /// <returns></returns>
    int detoxification(RoleInstance r1, RoleInstance r2)
    {
        if (r2.Poison > r1.DePoison + 20)
        {
            GameUtil.DisplayPopinfo("中毒太重无法解毒");
            return 0;
        }
        int add = (r1.DePoison / 3) + UnityEngine.Random.Range(0, 10) - UnityEngine.Random.Range(0, 10);
        int depoison = Tools.Limit(add, 0, r2.Poison);
        return depoison;
    }

    //暗器
    //返回值为一正数
    /// </summary>
    /// 暗器计算公式可以参考War_AnqiHurt：https://tiexuedanxin.net/forum.php?mod=viewthread&tid=394465
    ///
    /// 
    /// </summary>
    /// <param name="r1"></param>
    /// <param name="r2"></param>
    /// <param name="anqi"></param>
    /// <returns></returns>
    SkillCastResult hiddenWeapon(RoleInstance r1, RoleInstance r2, Jyx2ConfigItem anqi)
    {
        SkillCastResult rst = new SkillCastResult();
        //增加生命 = (暗器增加生命/a-random(5)-暗器能力*2)/3;
        //式中暗器增加生命为负值.
        //当受伤程度 = 100，a = 1;
        //当66 < 受伤程度 <= 99, a = 1;
        //当33 < 受伤程度 <= 66, a = 2;
        //当0 < 受伤程度 <= 33, a = 3;
        //当受伤程度 = 0, a = 4;
        int a = (int)Math.Ceiling((double)r2.Hurt / 33);
        if (a == 4) a = 3;
        int v = (anqi.AddHp / (4 - a) - UnityEngine.Random.Range(0, 5) - r1.Anqi * 2) / 3;
        rst.damage = -v;
        //敌人受伤程度
        rst.hurt = -v / 4; //此处v为负值
        //当暗器带毒 > 0,
        //增加中毒程度 = [(暗器带毒 + 暗器技巧) / 2 - 抗毒能力] / 2;
        //当抗毒 = 100, 增加中毒程度 = 0.
        if (anqi.ChangePoisonLevel > 0)
        {
            int add = ((anqi.ChangePoisonLevel + r1.Anqi) / 2 - r2.AntiPoison) / 2;
            if (r2.AntiPoison == 100)
                add = 0;
            int poison = Tools.Limit(add, 0, GameConst.MAX_USE_POISON);
            rst.poison = poison;
        }
        return rst;
    }
}
