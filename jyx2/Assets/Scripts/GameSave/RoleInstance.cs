using System;
using System.Collections.Generic;
using System.Reflection;
using Configs;
using UnityEngine;
using UniRx;
using Jyx2Configs;
using NUnit.Framework;
using Random = UnityEngine.Random;


namespace Jyx2
{
    //静态属性和一些动态的有初始值的属性放config,其余的放这里；载入存档时，必须要从存档取数，对于取不到的
    [Serializable]
    public class RoleInstance : IComparable<RoleInstance>
    {
        #region 存档数据定义
        
        //基本情况
        [SerializeField] public int Id; //ID
        [SerializeField] public string Name; //姓名
        [SerializeField] public String Sex; //性别
        [SerializeField] public String Race; //种族
        [SerializeField] public String Moral; //善恶
        [SerializeField] public String Describe; //描述
        [SerializeField] public int Level = 1; //等级
        [SerializeField] public int Exp; //经验
        
        //战斗属性
        [SerializeField] public int Hp;
        [SerializeField] public int MaxHp;
        [SerializeField] public int Mp;
        [SerializeField] public int MaxMp;
        [SerializeField] public int Hurt; //受伤程度
        [SerializeField] public String State; //受伤程度 中毒等
        
        [SerializeField] public int Attack; //攻击力
        [SerializeField] public int Defense; //防御力
        [SerializeField] public int Speed; //速度
        [SerializeField] public String Attach; //攻击附带
        [SerializeField] public int Critical; //暴击
        [SerializeField] public int CriticalLevel; //暴击伤害系数
        [SerializeField] public int Miss; //闪避
        [SerializeField] public int Heal; //恢复
        
        //资质
        [SerializeField] public int Strength; //力量
        [SerializeField] public int IQ; //智慧
        [SerializeField] public int Constitution; //体质
        [SerializeField] public int Agile; //敏捷
        [SerializeField] public int Luck; //幸运

        //携带
        [SerializeField] public List<SkillInstance> skills = new List<SkillInstance>(); //武功
        [SerializeField] public List<ItemInstance> Items = new List<ItemInstance>(); //道具
        [SerializeField] public List<ItemInstance> Equipments = new List<ItemInstance>(); //装备：0武器 1防具 2鞋子 4宝物
        
        [SerializeField] public int CurrentSkill = 0; //进入战斗时 默认选中的武功序号
        #endregion

        public ConfigCharacter configData;
        //额外增加的属性 确定要用后再加至存档 todo
        
        public int bestAttackDistance = 1;//最佳攻击距离，决定站位前后
        public bool isReadyToBattle = true;//是否参战
        public int currentMotivation  = 10; //当前行动力，可赋初始值
        public int motivationPerSecond = 10;//每秒获得的行动力
        public BattleBlockData blockData = new BattleBlockData();//当前所处格子坐标

        public RoleInstance()
        {
        }

        //初始化角色实例，从配置表中复制数据
        public RoleInstance(int roleId)
        {
            // 第一次初始化数据
            Id = roleId; 
            configData = GameConfigDatabase.Instance.Get<ConfigCharacter>(Id); 
            InitData(); //复制属性数据
        
            //添加配置的初始技能 配置技能数据+level ->存档的技能数据
            if (configData == null)Assert.Fail();
            if (skills.Count == 0)
            {
                if (configData.Skills == null || configData.Skills.Count == 0) //没配技能 加一个普攻技能
                {
                    skills.Add(new SkillInstance(0,0));
                }
                else
                {
                    foreach (var skill in configData.Skills)
                    {
                        skills.Add(new SkillInstance(skill.Skill.Id,skill.Level));
                    }
                }
            }
            //添加配置的初始物品
            Items.Clear();
            foreach (var item in configData.Items)
            {
                Items.Add(new ItemInstance(item.Item.Id,item.Count));
            }
            //添加配置的初始装备
            Equipments = new List<ItemInstance>() {null, null, null, null};
            foreach (var item in configData.Equipments)
            {
                if ( item != null &&  10 <= (int)item.ItemType && (int)item.ItemType < 20)
                {
                    var Equipment = new ItemInstance(item.Id, 1);
                    Equipments[(int)item.ItemType - 10] = Equipment;
                    Equipments[(int) item.ItemType - 10].UseRoleId = roleId;//使用人主要在物品中判断
                    Items.Add(Equipment);
                }
            }

            Recover(true);
        }
        
        public static T DeepCopy<T>(T obj)
        {
            //如果是字符串或值类型则直接返回
            if (obj is string || obj.GetType().IsValueType) return obj;
 
            object retval = Activator.CreateInstance(obj.GetType());
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                try { field.SetValue(retval, DeepCopy(field.GetValue(obj))); }
                catch { }
            }
            return (T)retval;
        }

        void InitData()
        {
            Name = configData.Name;
            Sex = configData.Sexual;
            Hp = configData.MaxHp;
            MaxHp = configData.MaxHp;
            Mp = configData.MaxMp;
            MaxMp = configData.MaxMp;
            Race = configData.Race;
            Moral = configData.Moral;
            Describe = configData.Descripe; 
            State = configData.State;
            
            Strength = configData.Strength;
            IQ = configData.IQ;
            Constitution = configData.Constitution;
            Agile = configData.Agile;
            
            Attack = configData.Attack;
            Speed = configData.Speed;
            Defense = configData.Defense;
            Heal = configData.Heal;
            Attach = configData.Attach;
            Critical = configData.Critical;
            CriticalLevel = configData.CriticalLevel;
            Miss = configData.Miss;
            Luck = configData.Luck;

            IQ = configData.IQ;
            
        }

        //获取技能
        public SkillInstance getSkill(int magicId)
        {
            foreach (var skill in skills)
            {
                if (skill.Key == magicId)
                {
                    return skill;
                }
            }

            return null;
        }
        
        public void Recover(bool condition)
        {
            if (condition)
            {
                SetHPAndRefreshHudBar(MaxHp);
                Mp = MaxMp;
                Hurt = 0;
            }
        }
        
        #region 等级相关
        
        //战斗完属性增长
        public bool CanLevelUp()
        {
            if (this.Level >= 1 && this.Level < GameConst.MAX_ROLE_LEVEL)
            {
                if (this.Exp >= GameConst._levelUpExpList[this.Level - 1])
                {
                    return true;
                }
            }

            return false;
        }
        
        public int GetLevelUpExp()
        {
            return GameConst._levelUpExpList[Level - 1];
        }
        
        public int ExpGot; //战斗中额外获得的经验

        #endregion

        /// 战斗中使用的招式
        public List<BattleZhaoshiInstance> Zhaoshis;
        
        /// 用于战斗中获取该角色蓝够的招式，（如果有医疗、用毒、解毒，也封装成招式）
        public IEnumerable<BattleZhaoshiInstance> GetZhaoshis(bool forceAttackZhaoshi)
        {
            foreach (var zhaoshi in Zhaoshis)
            {
                if (this.Mp >= zhaoshi.Data.MpCost)
                    yield return zhaoshi;
            }
            
            if (forceAttackZhaoshi)
                yield break;
        }

        //将角色实例的技能复制到角色实例的战斗技能 
        public void ResetBattleSkill()
        {
            if (Zhaoshis == null)
            {
                Zhaoshis = new List<BattleZhaoshiInstance>();
            }
            else
            {
                Zhaoshis.Clear();
            }

            foreach (var wugong in skills)
            {
                Zhaoshis.Add(new BattleZhaoshiInstance(wugong));
            }
        }

        #region 道具相关
        
        //获得随机装备,入包裹
        public void AddRanEquipment()
        {
            return;
        }
        
        //获取一个背包道具;装备由id唯一确定;物品由 configId + quality 唯一确定;isCertainId = false则为模糊获取
        public ItemInstance GetItem(String itemIdOrName,int quality = 0,Boolean isCertainId = true)
        {
            if (isCertainId)
            {
                return GameRuntimeData.Instance.Player.Items.Find(it => (itemIdOrName).Equals(it.Id)); //物品实例id获取
            }
            else 
            {
                var isEquipment = GameConfigDatabase.Instance.Get<ConfigItem>(itemIdOrName); //从配置表取物品类型
                if (isEquipment!= null && isEquipment.isEquipment()) 
                { 
                    return GameRuntimeData.Instance.Player.Items.Find(it => itemIdOrName.Equals(it.ConfigId) || itemIdOrName.Equals(it.Name));
                }
                else 
                {
                    return GameRuntimeData.Instance.Player.Items.Find(it => ( itemIdOrName.Equals(it.ConfigId) || itemIdOrName.Equals(it.Name) ) &&  quality == it.Quality);
                }
            }
            //获取失败
            
        }
        
        // 物品增减 + 已生成的装备增减
        /*
         物品  钱的加减 道具 类目id或名称+quality的加减
         装备 传全id
         */
        public void AlterItem(String itemConfigId,int count,int quality = 0)
        {
            var isEquipment = GameConfigDatabase.Instance.Get<ConfigItem>(itemConfigId).isEquipment(); //从配置表取物品类型
            
            var item = GetItem(itemConfigId,quality,false); //从背包获取
            if (count < 0 && item == null) return;//扣减物品不存在
            
            if (isEquipment) 
            {
                if (count > 0)
                {
                    Items.Add(new ItemInstance(int.Parse(itemConfigId),1,quality));
                }else
                {
                    //解除已携带关系
                    item.UseRoleId = -1;
                    Items.Remove(item);
                }
            }else
            {
                if ( item != null)
                {
                    item.Count += count;
                    if (count <  0 && item.Count <= 0)
                        Items.Remove(item);
                }
                else //新生成物品 从config取
                {
                    Items.Add(new ItemInstance(int.Parse(itemConfigId),count,quality));
                }
            }


        }
        
        public bool CanUseItem(String itemId)
        {
            var item = GetItem(itemId);
            if (item == null || item.ItemType == 0) return false;
            
            if (this.Strength - item.ConditionStrength< 0 || this.IQ - item.ConditionIQ< 0 || this.Agile - item.ConditionAgile < 0
                || this.Constitution - item.ConditionConstitution< 0 || this.Luck - item.ConditionLuck < 0 )
            {
                return false;
            }

            return true;
        }
        
        /// 使用物品
        public void UseItem(ItemInstance item)
        {
            if (item == null)
                return;
            
            int add = item.AddHp - this.Hurt / 2 + Random.Range(0, 10);
            if (add <= 0)
            {
                add = 5 + Random.Range(0, 5);
            }
            this.Hurt -= item.AddHp / 4;
            this.SetHPAndRefreshHudBar(this.Hp + add);
            this.MaxHp += item.AddMaxHp;
            this.Mp += item.AddMp;
            this.MaxMp += item.AddMaxMp;
            this.Heal += item.Heal;

            this.Attack += item.Attack;
            this.Defense += item.Defence;
            this.Speed += item.Speed;

            if (false)
            {
                this.LearnMagic(item.Skill);
            }
        }
        
        /// 卸下物品（装备） 解除装备与角色关系+角色属性增减+存储中的角色身上装备去除
        public void UnequipItem(ItemInstance item,int index)
        {
            if (item == null || item.Id == null)
                return;
            
            this.Equipments[index] = null; //存储中的角色身上装备去除
            GameRuntimeData.Instance.SetItemUser(item.Id, -1);// 解除装备与角色关系
            //角色属性增减
            this.SetHPAndRefreshHudBar(this.Hp - item.AddHp);
            this.MaxHp -= item.AddMaxHp;
            this.Mp -= item.AddMp;
            this.MaxMp -= item.AddMaxMp;
   
            this.Heal -= item.Heal;

            this.Attack -= item.Attack;
            this.Defense -= item.Defence;
            this.Speed -= item.Speed;
            
        }
        
        #endregion
        
        public MapRole View { get; set; }

        #region 战斗相关

        public BattleFieldModel BattleModel;

        //是否在战斗中
        private bool _isInBattle = false;

        //所属队伍，主角方为0
        public int team;

        //集气数量
        public float sp;

        //AI
        public bool isAI;

        private BattleBlockVector _pos;

        //位置
        public BattleBlockVector Pos
        {
            get { return _pos; }
            set
            {
                if (_pos == value)
                    return;
                _pos = value;
                UpdateViewPostion();
            }
        }

        public void UpdateViewPostion()
        {
            BattleBlockData posData = BattleboxHelper.Instance.GetBlockData(Pos.X, Pos.Y);
            View.SetPosition(posData.WorldPos);
        }
        
        //是否已经行动
        public bool isActed = false;
        public bool isWaiting = false; //正在等待

        public void EnterBattle()
        {
            if (_isInBattle) return;
            
            _isInBattle = true;

            View.LazyInitAnimator();

            //进入战斗时 默认选中的武功
            if (CurrentSkill >= skills.Count)
            {
                CurrentSkill = 0;
            }
            _currentSkill = skills[CurrentSkill];
            SwitchAnimationToSkill(_currentSkill, true);
        }

        public void SetHPAndRefreshHudBar(int hp)
        {
            Hp = hp;
            View?.MarkHpBarIsDirty();
        }

        private SkillInstance _currentSkill = null;

        public void SwitchAnimationToSkill(SkillInstance skill, bool force = false)
        {
            if (skill == null || (_currentSkill == skill && !force)) return;
            
            //切换武学待机动作
            View.SwitchSkillTo(skill);

            _currentSkill = skill;
        }

        public void LeaveBattle()
        {
            _isInBattle = false;
        }


        public void TimeRun()
        {
            IncSp();
        }

        //集气槽增长 根据轻功来增加
        public void IncSp()
        {
            sp += this.Speed / 4; //1f;
        }

        //获得行动力
        //参考：https://github.com/ZhanruiLiang/jinyong-legend
        public int GetMoveAbility()
        {
            int speed = this.Speed;

            speed = speed / 15 - this.Hurt / 40;

            if (speed < 0)
            {
                speed = 0;
            }
            return speed;
        }

        //是否是AI控制
        bool IsAI()
        {
            return isAI;
        }

        public int CompareTo(RoleInstance other)
        {
            int result = this.team.CompareTo(other.team);
            return result;
        }

        #endregion

        #region 状态相关
        
        public bool IsDead()
        {
            return Hp <= 0;
        }

        public void Resurrect()
        {
            SetHPAndRefreshHudBar(MaxHp);
        }

        //是否晕眩
        private bool _isStun = false;

        /// <summary>
        /// 晕眩
        /// </summary>
        /// <param name="duration">等于0时不晕眩；大于0时晕眩{duration}秒；小于0时永久晕眩</param>
        public void Stun(float duration = -1)
        {
            //记录晕眩状态
            if (duration > 0)
            {
                _isStun = true;
                View.ShowStun();
                int frame = Convert.ToInt32(duration * 60);
                Observable.TimerFrame(frame, FrameCountType.FixedUpdate)
                    .Subscribe(ms => { StopStun(); });
            }
            //永久晕眩（需要手动停止晕眩）
            else if (duration < 0)
            {
                _isStun = true;
                View.ShowStun();
            }
        }

        public void StopStun()
        {
            _isStun = false;
            View.StopStun();
        }

        //TODO:由于探索地图没有实例，所以晕眩状态暂时由UI决定 by Cherubinxxx
        public bool IsStun()
        {
            return _isStun;
        }

        #endregion

        //改为蓄力
        public void OnRest()
        {
            int addTili = 3 + Random.Range(0, 3);

        }

        //廉价的过度武学-可学习
        public int LearnMagic(int magicId)
        {
            if (magicId <= 0) return -1;
            SkillInstance skill = getSkill(magicId);
            Level = 1;
            if ( skill == null)
            {
                SkillInstance s = new SkillInstance(magicId);
                skills.Add(s);
            }else if (skill.Level < 3)
            {
                skill.Level = skill.Level + 1;
            }else
            {
                return -3;
            }
            return 0;
        }
        
        public string GetMPColor()
        {
            return ColorStringDefine.Default;
            //return MpType == 2 ? ColorStringDefine.Default : MpType == 1 ? ColorStringDefine.Mp_type1 : ColorStringDefine.Mp_type0;
        }
        
        public string GetHPColor1()
        {
            return Hurt > 20 ? ColorStringDefine.Hp_hurt_heavy : Hurt > 0 ? ColorStringDefine.Hp_hurt_light : ColorStringDefine.Default;
        }
        
        public string GetHPColor2()
        {
            return ColorStringDefine.Default;
            //return Poison > 0 ? ColorStringDefine.Hp_posion : ColorStringDefine.Default;
        }
        
        //根据传入名称 获取任意属性
        public int GetEquipmentProperty(string propertyName,int index)
        {
            if (this.Equipments.Count < index + 1 ) return 0;
            return this.Equipments[index] != null ? (int)Equipments[index].GetType().GetField(propertyName).GetValue(Equipments[index]) : 0;
        }
        
        /// 获取武器武功配合加攻击力
        /// 玄铁剑+玄铁剑法 攻击+100
        /// 君子剑+玉女素心剑 攻击+50
        /// 淑女剑+玉女素心剑 攻击+50
        /// 血刀+血刀大法 攻击+50
        /// 冷月宝刀+胡家刀法 攻击+70
        /// 金蛇剑+金蛇剑法 攻击力+80
        /// 霹雳狂刀+霹雳刀法 攻击+100
        public int GetExtraAttack(ConfigSkill wugong)
        {
            /*if (Equipments[0] !=null && Equipments[0].Id != -1 && this.Equipments[0].PairedWugong != null && this.Equipments[0].PairedWugong.Id == wugong.Id)
                return this.Equipments[0].ExtraAttack;*/
            return 0;

        }
    }
}
