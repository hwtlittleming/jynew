
using System;
using Cysharp.Threading.Tasks;
using Jyx2;
using Jyx2.MOD;
using Jyx2Configs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// 物品实例  有品质区别  装备可附魂 
    [Serializable]
    public class ItemInstance
    {
        #region 存档数据定义
        [SerializeField] public String Id;
        [SerializeField] public String Name;
        [SerializeField] public int ConfigId;
        //动态数据
        [SerializeField] public int UseRoleId;//使用人id
        [SerializeField] public String Desc;//物品说明
        [SerializeField] public int ItemType;//物品类型
        [SerializeField] public int Skill; //习得技能
        [SerializeField] public int AddHp; //加生命
        [SerializeField] public int AddMaxHp; //加生命最大值
        [SerializeField] public int AddMp; //加内力
        [SerializeField] public int AddMaxMp; //加内力最大值
        [SerializeField] public int Attack; //加攻击力
        [SerializeField] public int Defence; //加防御力
        [SerializeField] public int Speed; //加速度
        [SerializeField] public int Heal; //加回复
        
        //装备扩展数据
        [SerializeField] public int bestDistance; //最佳攻击距离
        [SerializeField] public int attackRange; //攻击范围，名武器独有

        [SerializeField] public int ConditionStrength; //需力量
        [SerializeField] public int ConditionIQ; //需智慧
        [SerializeField] public int ConditionConstitution; //需体质
        [SerializeField] public int ConditionAgile; //需敏捷
        [SerializeField] public int ConditionLuck; //需幸运

        [SerializeField] public int Count; //数量
        [SerializeField] public int Quality; //品质
        [SerializeField] public String imagePath; //图片真实路径
        [SerializeField] public Sprite sprite; //图片
        //技能等级升级后属性变化方法，携带道具类 换成xx instance
        #endregion
       
        public ItemInstance()
        {
        }
        
        //用来从配置拿一个初始物品的方法，quality = -1随机生成,0默认最低级品质 
        public ItemInstance(int configId,int count = 1,int quality = 0)
        {
            //1.取配置的默认值
            Jyx2ConfigItem configItem = GameConfigDatabase.Instance.Get<Jyx2ConfigItem>(configId);
            //装备生成随机Id 不堆叠, 物品由 configId+品质来堆叠
            if ( 10<= (int)configItem.ItemType &&  (int)configItem.ItemType < 20  )
            {
                Id = "Equipment_" + configId.ToString() + "_" + Guid.NewGuid().ToString(); 
            }
            else
            {
                Id = configItem.Quality +  "_"  + configId.ToString() ; 
            }
            
            Name = configItem.Name;
            ConfigId = configItem.Id;
            Count = count;
            UseRoleId = -1;
            Desc = configItem.Desc;
            ItemType = (int)configItem.ItemType;
            imagePath = Jyx2ResourceHelper.GetAssetRefAddress(configItem.Pic, typeof(Texture2D));
            
            Skill = configItem.Skill;
            Quality = configItem.Quality;
            AddHp = configItem.AddHp;
            AddMaxHp = configItem.AddMaxHp;
            AddMp = configItem.AddMp;
            AddMaxMp = configItem.AddMaxMp;
            Attack = configItem.Attack;
            Defence = configItem.Defence;
            Speed = configItem.Speed;
            Heal = configItem.Heal;
            ConditionStrength = configItem.ConditionStrength;
            ConditionIQ = configItem.ConditionIQ;
            ConditionConstitution = configItem.ConditionConstitution;
            ConditionAgile = configItem.ConditionAgile;
            ConditionLuck = configItem.ConditionLuck;

            //2.进行实例化替换 
            ChangeForLevel();
        }

        public void ChangeForLevel()
        {
            
            return;
        }
        
        public async UniTask<Sprite> GetPic()
        {
            if (sprite == null)
            {
                sprite = await MODLoader.LoadAsset<Sprite>(imagePath);
            }
            return sprite;
        }
        
        //随机生成 带品质的物品 装备
        public void GenerateItem()
        {
            return ;
        }
    }

