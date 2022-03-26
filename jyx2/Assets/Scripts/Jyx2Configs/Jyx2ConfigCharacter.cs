using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Jyx2;
using Jyx2.MOD;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Jyx2Configs
{
    [CreateAssetMenu(menuName = "金庸重制版/配置文件/角色", fileName = "角色ID_角色名")]
    public class Jyx2ConfigCharacter : Jyx2ConfigBase
    {
        
        /// <summary>
        /// 设置为中文仅用于odin显示，不要在代码中直接使用
        /// </summary>
        public enum SexualType
        {
            男 = 0,
            女 = 1
        }

        /// <summary>
        /// 设置为中文仅用于odin显示，不要在代码中直接使用
        /// </summary>
        //0:阴 1:阳 2:调和
        public enum RateTypeEnum
        {
            人 = 0,
            妖 = 1,
            魔 = 2,
            仙 = 3,
            神 = 4
        }

        private const string CGroup1 = "基本配置";
        private const string CGroup2 = "属性";
        private const string CGroup3 = "资质";
        private const string CGroup4 = "装备";
        private const string CGroupSkill = "武功";
        private const string CGroupItems = "道具";
        
        [BoxGroup(CGroup1)][LabelText("性别")][EnumToggleButtons] 
        public SexualType Sexual;
        
        [BoxGroup(CGroup1)][LabelText("种族")][EnumToggleButtons]
        public RateTypeEnum Rate;
        
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
        
        [BoxGroup(CGroup1)][LabelText("善恶")] 
        public int Moral; //善恶
        
        [BoxGroup(CGroup1)][LabelText("智商")] 
        public int IQ; //智商
        
        [BoxGroup(CGroup1)][LabelText("描述")] 
        public String Descripe; //描述
        
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
        
        [BoxGroup(CGroup2)][LabelText("法术攻击")]
        public int MagicAttack; //法术攻击
        
        [BoxGroup(CGroup2)][LabelText("速度")]
        public int Speed; //轻功
        
        [BoxGroup(CGroup2)][LabelText("防御力")]
        public int Defense; //防御力
        
        [BoxGroup(CGroup2)][LabelText("魔抗")]
        public int MagicDefence; //魔抗
        
        [BoxGroup(CGroup2)][LabelText("回复力")]
        public int Heal; //回复力

        [BoxGroup(CGroup2)][LabelText("攻击附带")]
        public String Attach; //攻击附带
        
        [BoxGroup(CGroup2)][LabelText("暴击")]
        public int Critical; 
        
        [BoxGroup(CGroup2)][LabelText("暴击伤害系数")]
        public int CriticalLevel; 
        
        [BoxGroup(CGroup2)][LabelText("闪避")]
        public int Miss; 
        
        [BoxGroup(CGroup2)][LabelText("幸运")]
        public int Luck; 

        /* ------- 分割线 --------*/
        
        [BoxGroup(CGroup3)][LabelText("力量")][SerializeReference]
        public int Strength; //中量提高生命，提高物攻
        
        [BoxGroup(CGroup3)][LabelText("智慧")][SerializeReference]
        public int Intelligence;//少量提升生命，提高法攻
        
        [BoxGroup(CGroup3)][LabelText("根骨")][SerializeReference]
        public int Constitution;//提高生命和能量，少量物防
        
        [BoxGroup(CGroup3)][LabelText("敏捷")][SerializeReference]
        public int Agile;//提高出手速度，少量提升闪避几率
        
        /* ------- 分割线 --------*/
        
        [BoxGroup(CGroup4)][LabelText("武器")][SerializeReference]
        public Jyx2ConfigItem Weapon;
        
        [BoxGroup(CGroup4)][LabelText("衣服")][SerializeReference]
        public Jyx2ConfigItem Armor;
        
        [BoxGroup(CGroup4)][LabelText("鞋子")][SerializeReference]
        public Jyx2ConfigItem Shoes;
        
        [BoxGroup(CGroup4)][LabelText("饰品")][SerializeReference]
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

