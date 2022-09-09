using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Jyx2;
using Jyx2.MOD;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jyx2Configs
{
    public enum Jyx2ConfigItemType
    {
        剧情道具 = 0,
        技能书 = 2, 
        消耗品 = 3, 
       
        //装备
        武器 = 10, 
        防具 = 11, 
        代步 = 12, 
        宝物 = 13, 
    }
    [Serializable]
    [CreateAssetMenu(menuName = "金庸重制版/配置文件/道具", fileName = "道具ID_道具名")]
    public class Jyx2ConfigItem : Jyx2ConfigBase
    {
        
        [BoxGroup(DEFAULT_GROUP_NAME)][LabelText("图标")]
        public AssetReferenceTexture2D Pic;
        private Sprite _sprite;
        public async UniTask<Sprite> GetPic()
        {
            if (Pic == null || string.IsNullOrEmpty(Pic.AssetGUID)) return null;
            if (_sprite == null)
            {
                _sprite = await MODLoader.LoadAsset<Sprite>(Jyx2ResourceHelper.GetAssetRefAddress(Pic, typeof(Texture2D)));
            }
            return _sprite;
        }
        
        public int bestDistance;//最佳攻击距离
        public String attackRange;//攻击范围，名武器独有,最多仅横纵
        
        private const string EXTEND_GROUP = "扩展属性";
        private const string EFFECT_GROUP = "使用效果";
        private const string CONDITION_GROUP = "使用条件";
        
        [BoxGroup(EXTEND_GROUP)][LabelText("武器武功配合加攻击力")]
        public int ExtraAttack;
        
        //某装备和武功配合 使得
        [BoxGroup(EXTEND_GROUP)][LabelText("配合武功")][SerializeReference]
        public Jyx2ConfigSkill PairedWugong;

        [BoxGroup(DEFAULT_GROUP_NAME)][LabelText("物品说明")]
        public string Desc; 
        
        [BoxGroup(DEFAULT_GROUP_NAME)][LabelText("物品类型")][EnumToggleButtons]
        public Jyx2ConfigItemType ItemType;
        
        [BoxGroup(DEFAULT_GROUP_NAME)] [LabelText("品质")] //物品增益效果根据品质提升百分比；装备根据品质属性增幅
        public String Quality;
        
        [ShowIf(nameof(ShowSkill))]
        [BoxGroup(DEFAULT_GROUP_NAME)][LabelText("练出武功")][SerializeReference]
        public Jyx2ConfigSkill Skill;

        bool ShowSkill()
        {
            return (int)ItemType == 2;
        }
        // todo 资质的东西，宝物的临时增加资质
        [BoxGroup(EFFECT_GROUP)][LabelText("加生命")] //战斗中的伤药
        public int AddHp; 

        [BoxGroup(EFFECT_GROUP)][LabelText("加生命最大值")]
        public int AddMaxHp;

        [BoxGroup(EFFECT_GROUP)][LabelText("加内力")]//战斗中的伤药
        public int AddMp;

        [BoxGroup(EFFECT_GROUP)][LabelText("加内力最大值")]
        public int AddMaxMp;

        [BoxGroup(EFFECT_GROUP)][LabelText("加攻击力")]
        public int Attack;

        [BoxGroup(EFFECT_GROUP)][LabelText("加速度")]
        public int Speed;

        [BoxGroup(EFFECT_GROUP)][LabelText("加防御力")]
        public int Defence;

        [BoxGroup(EFFECT_GROUP)][LabelText("加回复")]
        public int Heal;
        
        
        [BoxGroup(CONDITION_GROUP)][LabelText("需力量")]
        public int ConditionStrength;
        
        [BoxGroup(CONDITION_GROUP)][LabelText("需智慧")]
        public int ConditionIQ;

        [BoxGroup(CONDITION_GROUP)][LabelText("需体质")]
        public int ConditionConstitution;
        
        [BoxGroup(CONDITION_GROUP)][LabelText("需敏捷")]
        public int ConditionAgile;
        
        [BoxGroup(CONDITION_GROUP)][LabelText("需幸运")] //宝物需要幸运
        public int ConditionLuck;
        
        
        public override async UniTask WarmUp()
        {
            //GetPic().Forget();
            //清理缓存
            if (Application.isEditor)
            {
               // _sprite = null;
            }
        }
    }
}
