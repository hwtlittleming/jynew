using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    //这里配置1阶技能初始值 todo 技能等级增长带来属性增长的记录
    [CreateAssetMenu(menuName = "配置文件/技能", fileName = "技能ID_技能名")]
    public class ConfigSkill : ConfigBase
    {
        public enum Jyx2ConfigSkillDamageType
        {
            普通 = 0,
            吸能 = 1,
            用毒 = 2,
            解毒 = 3,
            治疗 = 4,
            封魔 = 5,
            混乱 = 6,
            灼烧 = 7,
            冰冻 = 8,
        }

        public enum Jyx2ConfigSkillCoverType
        {
            点攻击 = 0,
            横排攻击 = 1,
            竖排攻击 = 2,
            随机斜线攻击 = 3,
            邻近格子攻击 = 4,
            全体攻击 = 5 
        }
        
        public enum ConfigSkillLevelType
        {
            初窥门径 = 1,
            驾轻就熟 = 2,  //属性增加，伤害增幅，范围增减
            心领神会 = 3, //额外附加效果 红警三阶标志
        }
        
        private const string CGroup1 = "基本配置";
        private const string CGroup2 = "战斗属性";

        [BoxGroup(CGroup2)][LabelText("技能等阶")][EnumToggleButtons]
        public ConfigSkillLevelType SkillLevel ;
        
        [BoxGroup(CGroup2)][LabelText("伤害类型")][EnumPaging]
        public Jyx2ConfigSkillDamageType DamageType; //伤害类型
        
        [BoxGroup(CGroup2)][LabelText("攻击范围类型")][EnumPaging]
        public Jyx2ConfigSkillCoverType SkillCoverType; //攻击范围
        
        [BoxGroup(CGroup2)][LabelText("消耗能量点数")]
        public int MpCost;

        [BoxGroup(CGroup2)][LabelText("固定伤害")]
        public int FixedDamage = 0;
        
        [BoxGroup(CGroup2)][LabelText("技能伤害系数")]
        public int DamageLevel;
        
        [BoxGroup(CGroup2)][LabelText("作用于 0己方 1敌人")]
        public int ToWhichSide = 1;
        
        [InlineEditor] [BoxGroup("技能外观")] [SerializeReference]
        public SkillDisplayAsset Display;

        public override async UniTask WarmUp()
        {
            
        }
    }
}
