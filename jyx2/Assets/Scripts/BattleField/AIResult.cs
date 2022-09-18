
using System.Xml.Serialization;
using Jyx2.Middleware;

namespace Jyx2
{
    [XmlType]
    public class AIResult
    {
        #region 行为结果
        
        public AIResult(RoleInstance sprite, RoleInstance target)
        {
            r1 = sprite;
            r2 = target;
        }
        
        
        //攻击坐标
        [XmlAttribute]
        public int AttackX;

        [XmlAttribute]
        public int AttackY;

        //是否休息
        [XmlAttribute]
        public bool IsRest;

        //使用的道具
        [XmlAttribute]
        public ItemInstance Item;
        #endregion
        [XmlIgnore]
        
        public SkillInstance zhaoshi;

        [XmlIgnore]
        public RoleInstance r1;

        [XmlIgnore]
        public RoleInstance r2;

        public int damage; //伤害
        public int damageMp;
        public int addMp; //增加内力
        public int addMaxMp;
        public int heal;
        public int hurt;

        public void Run()
        {
            var rst = this;
            if (rst.damage > 0)
            {
                r2.Hp -= rst.damage;

                if (r2.View != null)
                {
                    r2.View.SetDamage(rst.damage);
                }

                r1.ExpGot += 2 + rst.damage / 5;
                //打死敌人获得额外经验
                if (r2.Hp <= 0)
                    r1.ExpGot += r2.Level * 10;

                //无敌
                if(BattleManager.Whosyourdad && r2.team == 0)
                {
                    r2.Hp = r2.MaxHp;
                }
            }

            if (rst.damageMp > 0)
            {
                int damageMp = Tools.Limit(rst.damageMp, 0, r2.Mp);
                r2.Mp -= damageMp;
                if (r2.View != null)
                {
                    r2.View.ShowAttackInfo($"<color=blue>内力-{damageMp}</color>");
                }

                //吸取内力逻辑
                if (rst.addMp > 0)
                {
                    r1.MaxMp = Tools.Limit(r1.MaxMp + rst.addMaxMp, 0, GameConst.MAX_HPMP);
                    int finalMp = Tools.Limit(r1.Mp + rst.addMp, 0, r1.MaxMp);
                    int deltaMp = finalMp - r1.Mp;
                    if (deltaMp >= 0)
                    {
                        r1.View.ShowAttackInfo($"<color=blue>内力+{deltaMp}</color>");
                        r1.Mp = finalMp;
                    }
                }
            }

            if (rst.heal > 0)
            {
                int tmp = r2.Hp;
                r2.Hp += rst.heal;
                r2.Hp = Tools.Limit(r2.Hp, 0, r2.MaxHp);
                int addHp = r2.Hp - tmp;
                if (r2.View != null)
                {
                    r2.View.ShowAttackInfo($"<color=white>医疗+{addHp}</color>");
                }

                r1.ExpGot += 1;
            }

            r2.Hurt += rst.hurt;
            r2.Hurt = Tools.Limit(r2.Hurt, 0, GameConst.MAX_HURT);
        }
    }
}
