using Jyx2.Middleware;
using Random = UnityEngine.Random;

namespace Jyx2
{
    public class BattleZhaoshiInstance
    {
        public const int MAX_MAGIC_LEVEL_INDEX = 9;
        
        public string Key;
        public BattleZhaoshiInstance(SkillInstance skill)
        {
            Data = skill;
            Key = skill.Key.ToString();
        }

        public SkillInstance Data
        {
            get; 
            set;
        }
        
        /// JYX2使用技能消耗的内力
        public void CastMP(RoleInstance role)
        {
            int damageType = GetDamageType();
            if (damageType == 0 || damageType == 1)//普通攻击、吸内
            {
                role.Mp = Tools.Limit(role.Mp - Data.MpCost, 0, role.MaxMp);

                role.ExpGot += 2;

                int levelAdd = Tools.Limit(1 + Random.Range(0, 2), 0, 100 * 10);

                //空挥升级
                if ((Data.Level / 100) < ((Data.Level + levelAdd) / 100))
                {
                    StoryEngine.Instance.DisplayPopInfo(
                        $"{role.Name}的{this.Data.Name}升到{((Data.Level + levelAdd) / 100) + 1}级!");
                }

                //JYX2:最多10级，每级100
                this.Data.Level += levelAdd;
                this.Data.Level = Tools.Limit(this.Data.Level, 0, GameConst.MAX_SKILL_LEVEL);
            }
        }
        
        public virtual int GetDamageType()
        {
            return (int)Data.DamageType;
        }
    }

}
