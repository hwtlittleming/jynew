
using System;
using Jyx2Configs;
using UnityEngine;

namespace Jyx2
{
    /// 技能实例  不同角色掌握程度不同，因此不能用静态的；静态的属性和有默认值的动态属性放Config中，每次去asset取固定的，动态的属性放这里
    [Serializable]
    public class SkillInstance
    {
        #region 存档数据定义
        [SerializeField] public int Key;
        [SerializeField] public String Name;
        //动态数据
        [SerializeField] public int ConfigId;
        [SerializeField] public int Level;
        [SerializeField] public int MpCost; //能量消耗
        [SerializeField] public int DamageType; //伤害的特殊效果
        [SerializeField] public int SkillCoverType; //攻击范围
        [SerializeField] public int FixedDamage; //固定伤害
        [SerializeField] public int DamageLevel; //技能伤害系数
        [SerializeField] public int DisplayId; //技能外观ID
        
        //技能等级升级后属性变化方法，携带道具类 换成xx instance
        #endregion

        //技能外观 非存档数据 只要不加[SerializeField] ES3就不会存档；只要初始化时和变更时同时给其赋值就可以了
        public Jyx2SkillDisplayAsset Display;
        
        public SkillInstance()
        {
        }
        
        public SkillInstance(int configId)
        {
            //1.取配置的默认值
            Key = configId;
            ConfigId = configId;
            Jyx2ConfigSkill configSkill = GameConfigDatabase.Instance.Get<Jyx2ConfigSkill>(ConfigId);
            
            Name = configSkill.Name;
            Level = 0;
            MpCost = configSkill.MpCost;
            DamageType = (int)configSkill.DamageType;
            SkillCoverType = (int)configSkill.SkillCoverType;
            FixedDamage = configSkill.FixedDamage;
            DamageLevel = configSkill.DamageLevel;
            DisplayId = configSkill.Display.Id;
            Display = GameConfigDatabase.Instance.Get<Jyx2SkillDisplayAsset>(DisplayId);
            
            //2.进行实例化替换
            Refreshskill();
        }

        public void Refreshskill()
        {
            return;
        }

        public Jyx2ConfigSkill GetSkill(Jyx2ConfigItem _anqi = null)
        {
            var skillT = GameConfigDatabase.Instance.Get<Jyx2ConfigSkill>(Key);
            
            return skillT;
        }

        public int GetCoolDown()
        {
            return 0;
        }
    }

}
