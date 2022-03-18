using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Jyx2Configs
{
    [CreateAssetMenu(menuName = "金庸重制版/配置文件/战斗", fileName = "战斗ID")]
    public class Jyx2ConfigBattle : Jyx2ConfigBase
    {
        public static Jyx2ConfigBattle Get(int id)
        {
            return GameConfigDatabase.Instance.Get<Jyx2ConfigBattle>(id);
        }
        
        [InfoBox("引用指定战斗场景asset")]
        [LabelText("地图")] 
        public AssetReference MapScene;

        [LabelText("获得经验")] 
        public int Exp;
        
        [LabelText("音乐")]
        public AssetReferenceT<AudioClip> Music; //音乐

        [BoxGroup("战斗人物设置")] [LabelText("队友")] [SerializeReference]
        public List<Jyx2ConfigCharacter> TeamMates;
        
        [BoxGroup("战斗人物设置")] [LabelText("自动队友")] [SerializeReference]
        public List<Jyx2ConfigCharacter> AutoTeamMates;
        
        [BoxGroup("战斗人物设置")] [LabelText("固定的战斗敌人")] [SerializeReference]
        public List<Jyx2ConfigCharacter> Enemies;
        
        //传入的战斗地图id命名规则，结尾0/1/2
        //每个地图或线路 0:随机战斗，每个地图只有一种随机战斗；1:NPC单挑，1名敌人，读传入的roleId;2:自由设计战斗，自由设置敌人，可定义多种配置；0和2都可复用后面字段
        [BoxGroup("新增内容")] [LabelText("战斗类型")] [SerializeReference]
        public String BattleKind;

        [BoxGroup("新增内容")] [LabelText("数量等级(随机战斗专用)")] [SerializeReference]
        public String CountLevel;

        [BoxGroup("新增内容")] [LabelText("出现各敌人的概率(roleid:rate)")] [SerializeReference]
        public List<SamepleRate> RoleRate;
        
        public override async UniTask WarmUp()
        {
            
        }
    }
    
    [Serializable]
    public class SamepleRate : IComparable<SamepleRate>
    {
        [LabelText("样本")] 
        public String Sameple;
        
        [LabelText("概率")]
        public int Rate;
        public int CompareTo(SamepleRate obj)
        {
            return Rate.CompareTo(obj.Rate);
        }
        
    }
}