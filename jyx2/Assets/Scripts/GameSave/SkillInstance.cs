
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
        //动态数据
        [SerializeField] public int Level;
        [SerializeField] public int MpCost; //能量消耗
        [SerializeField] public int DamageType; //伤害的特殊效果
        [SerializeField] public int SkillCoverType; //攻击范围
        [SerializeField] public int FixedDamage; //固定伤害
        [SerializeField] public int DamageLevel; //技能伤害系数
        [SerializeField] public int DisplayId; //技能外观ID
        
        //技能等级升级后属性变化方法，携带道具类 换成xx instance
        #endregion
        public SkillInstance()
        {
        }

        public SkillInstance(Jyx2ConfigCharacterSkill s)
        {
            Key = s.Skill.Id;
            Level = s.Level;
        }
        
        public SkillInstance(int magicId)
        {
            Key = magicId;
            Level = 0;
            GetSkill();
        }

        public SkillInstance(Jyx2ConfigItem item, int magicId)
        {
            Key = magicId;
            Level = 0;
            GetSkill(item);
        }

        public int GetLevel()
        {
            return Level / 100 + 1;
        }

        public string Name
        {
            get
            {
                return GetSkill().Name;
            }
        }

        public Jyx2ConfigSkill GetSkill(Jyx2ConfigItem _anqi = null)
        {
            var skillT = GameConfigDatabase.Instance.Get<Jyx2ConfigSkill>(Key);
            
            return skillT;
        }

        public void ResetSkill()
        {
            _skill = null;
        }

        Jyx2ConfigSkill skill;
        Jyx2ConfigSkill _skill{
			get {
				if(skill==null) skill=GetSkill();
				return skill;
			}
			set {
				skill=value;
			}
		}
        
        public Jyx2SkillDisplayAsset GetDisplay()
        {
			return _skill.Display;
        }

        public int GetCoolDown()
        {
            return 0;
        }
    }

}
