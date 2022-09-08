using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Jyx2;
using Jyx2.MOD;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jyx2Configs
{
    [CreateAssetMenu(menuName = "金庸重制版/配置文件/角色", fileName = "角色ID_角色名")]
    public class Jyx2ConfigCharacter : Jyx2ConfigBase
    {
        private const string CGroup1 = "基本信息";
        private const string CGroup2 = "属性";
        private const string CGroup3 = "资质";
        private const string CGroup4 = "装备";
        private const string CGroupSkill = "武功";
        private const string CGroupItems = "道具";
        
        [BoxGroup(CGroup1)][LabelText("性别")][EnumToggleButtons] 
        public String Sexual;
        
        [BoxGroup(CGroup1)][LabelText("种族/职业")][EnumToggleButtons]
        public String Race;
        
        [BoxGroup(CGroup1)][LabelText("头像")]
        public AssetReferenceTexture2D Pic;

        private Sprite _sprite;
        public async UniTask<Sprite> GetPic()
        {
            if (Pic == null|| string.IsNullOrEmpty(Pic.AssetGUID)) return null;
            
            if (_sprite == null)
            {
                var path = Jyx2ResourceHelper.GetAssetRefAddress(Pic, typeof(Texture2D)); //先转换到URL
                _sprite = await MODLoader.LoadAsset<Sprite>(path); //在MOD列表中过滤
            }
            return _sprite;
        }
        
        [BoxGroup(CGroup1)][LabelText("善恶(不同地域的仇恨记录)")] 
        public String Moral; //善恶

        [BoxGroup(CGroup1)][LabelText("描述")] 
        public String Descripe; //描述
        
        [BoxGroup(CGroup1)][LabelText("状态")] 
        public String State; //状态
        
        /* ------- 分割线 --------*/
        
        [BoxGroup(CGroup3)][LabelText("力量")][SerializeReference]
        public int Strength; //0.2提高生命，0.8提高物攻
        
        [BoxGroup(CGroup3)][LabelText("智慧")][SerializeReference]
        public int IQ;//0.3 提升生命，0.01提高物理法术暴击率，0.1赚钱倍率，多次使用某技能概率升级
        
        [BoxGroup(CGroup3)][LabelText("体质")][SerializeReference]
        public int Constitution;//0.5生命 0.5防御，0.1回复
        
        [BoxGroup(CGroup3)][LabelText("敏捷")][SerializeReference]
        public int Agile;// 1.0速度,0.01闪避,0.2防御
        
        [BoxGroup(CGroup3)][LabelText("幸运")][SerializeReference]
        public int Luck;//+点数点闪避,事件触发 
        
        /* ------- 分割线 ------- */
        
        [InfoBox("必须至少有一个技能", InfoMessageType.Error, "@this.Skills==null || this.Skills.Count == 0")]
        [InfoBox("注：等级0：对应1级技能，  等级900：对应10级技能")]
        [BoxGroup(CGroupSkill)] [LabelText("技能")][SerializeReference][TableList]
        public List<Jyx2ConfigCharacterSkill> Skills;
        
        /* ------- 分割线 --------*/
        [BoxGroup(CGroupItems)] [LabelText("携带道具")][TableList]
        public List<Jyx2ConfigCharacterItem> Items;

        
        /* ------- 分割线 --------*/

        [BoxGroup(CGroup2)][LabelText("生命上限")]
        public int MaxHp;
        
        [BoxGroup(CGroup2)][LabelText("能量上限")]
        public int MaxMp;

        [BoxGroup(CGroup2)][LabelText("攻击力")]
        public int Attack; //攻击力
        
        [BoxGroup(CGroup2)][LabelText("防御力")]
        public int Defense; //防御力 减免1/100分制

        [BoxGroup(CGroup2)][LabelText("速度")]
        public int Speed; //速度

        [BoxGroup(CGroup2)][LabelText("回复力")]
        public int Heal; //每次回复

        [BoxGroup(CGroup2)][LabelText("暴击率")]
        public int Critical; 
        
        [BoxGroup(CGroup2)][LabelText("暴击伤害系数")]
        public int CriticalLevel; 
        
        [BoxGroup(CGroup2)][LabelText("闪避率")]
        public int Miss;
        
        [BoxGroup(CGroup2)][LabelText("攻击附带")]
        public String Attach; //攻击附带

        [BoxGroup(CGroup2)][LabelText("战斗经验")] //经验每满一定程度 属性按资质增长，宠物可额外通过吞食获得经验
        public int Exp;

        /* ------- 分割线 --------*/
        
        [BoxGroup(CGroup4)][LabelText("武器")][SerializeReference]
        public Jyx2ConfigItem Weapon;
        
        [BoxGroup(CGroup4)][LabelText("防具")][SerializeReference]
        public Jyx2ConfigItem Armor;
        
        [BoxGroup(CGroup4)][LabelText("代步")][SerializeReference]
        public Jyx2ConfigItem Shoes;
        
        [BoxGroup(CGroup4)][LabelText("宝物")][SerializeReference]
        public Jyx2ConfigItem Treasure;
        /* ------- 分割线 --------*/

        [BoxGroup("其他")][LabelText("队友离场对话")] 
        public string LeaveStoryId;

        /* ------- 分割线 --------*/
        
        [BoxGroup("模型配置")] [LabelText("模型配置")] [SerializeReference][InlineEditor]
        public ModelAsset Model;

        public override async UniTask WarmUp()
        {
            //GetPic().Forget();
            
            //清理缓存
            if (Application.isEditor)
            {
                _sprite = null;
            }
        }
    }

    [Serializable]
    public class Jyx2ConfigCharacterSkill
    {
        [LabelText("技能")][SerializeReference][InlineEditor]
        public Jyx2ConfigSkill Skill;

        [LabelText("等级")] 
        public int Level;
    }
    
    [Serializable]
    public class Jyx2ConfigCharacterItem
    {
        [LabelText("道具")][SerializeReference][InlineEditor]
        public Jyx2ConfigItem Item;

        [LabelText("数量")] 
        public int Count;
        
    }
}

