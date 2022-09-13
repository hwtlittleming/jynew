using Jyx2.Middleware;
using Random = UnityEngine.Random;

namespace Jyx2
{
    public class BattleZhaoshiInstance
    {
        public const int MAX_MAGIC_LEVEL_INDEX = 9;
        
        public BattleZhaoshiInstance(SkillInstance skill)
        {
            Data = skill;
            level = skill.Level;
            Key = skill.Key.ToString();
        }

        public enum ZhaoshiStatus
        {
            OK, //正常
            CD, //CD中
        }

        public SkillInstance Data
        {
            get; 
            set;
        }
        
        public string Key;

        public int level;

        public int CurrentCooldown = 0;

        public static int TimeTickCoolDown = GameConst.ACTION_SP;

        public void CastCD()
        {
            CurrentCooldown += Data.GetCoolDown() * TimeTickCoolDown;
        }

        /// <summary>
        /// JYX2使用技能消耗的内力
        /// </summary>
        public void CastMP(RoleInstance role)
        {
            int damageType = GetDamageType();
            if (damageType == 0 || damageType == 1)//普通攻击、吸内
            {
                int level_index = this.Data.Level;
                role.Mp = Tools.Limit(role.Mp - this.calNeedMP(level_index), 0, role.MaxMp);

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

        /// <summary>
        /// JYX2使用技能消耗的逻辑（体力、道具）
        /// </summary>
        public void CastCost(RoleInstance role)
        {
            int damageType = GetDamageType();
            /*if(damageType == 0 || damageType == 1)//普通攻击、吸内
            {
                role.Tili = Tools.Limit(role.Tili - 3, 0, 100);
            }else if(damageType ==2)//用毒
            {
                role.Tili = Tools.Limit(role.Tili - 2, 0, 100);
            }else if(damageType == 3)//解毒
            {
                role.Tili = Tools.Limit(role.Tili - 2, 0, 100);
            }else if(damageType == 4)//医疗
            {
                role.Tili = Tools.Limit(role.Tili - 4, 0, 100);
            }*/
            
            /*//暗器，扣除道具
            if (this is AnqiZhaoshiInstance)
            {
                if (!role.isAI)
                {
                    GameRuntimeData.Instance.AddItem(Anqi.Id, -1);
                }
                else
                {
                    role.AddItem(Anqi.Id, -1);
                }
            }*/
        }

        /// <summary>
        /// JYX2:Magic int calNeedMP(int level_index) { return NeedMP * ((level_index + 1) / 2); }
        /// </summary>
        /// <param name="level_index"></param>
        /// <returns></returns>
        public int calNeedMP(int level_index) { return Data.MpCost * ((level_index + 1) / 2); }

        public void TimeRun()
        {
            if (CurrentCooldown > 0)
                CurrentCooldown -= 1;
        }

        public ZhaoshiStatus GetStatus()
        {
            if (CurrentCooldown > 0)
                return ZhaoshiStatus.CD;
            return ZhaoshiStatus.OK;
        }


        public virtual bool IsCastToEnemy()
        {
            return true;
        }


        public int calMaxLevelIndexByMP(int mp, int max_level)
        {
            max_level = limit(max_level, 0, MAX_MAGIC_LEVEL_INDEX);
            int needMp = Data.MpCost;
            if(needMp <= 0)
            {
                return max_level;
            }
            int level = limit(mp / (needMp * 2) * 2 - 1, 0, max_level);
            return level;
        }

        public int getMpCost()
        {
            int needMp = Data.MpCost;
            return needMp * (level + 2) / 2; // 内力消耗计算公式
        }

        private int limit(int v,int v1,int v2)
        {
            if (v < v1) v = v1;
            if (v > v2) v = v2;
            return v;
        }
        
        public virtual int GetDamageType()
        {
            return (int)Data.DamageType;
        }
    }

}
