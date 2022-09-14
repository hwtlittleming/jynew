using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Configs
{
    //大体静态，可增加属性记录实现 到达某条件 某些值就用新的
    [CreateAssetMenu(menuName = "配置文件/战斗", fileName = "战斗ID")]
    public class ConfigBattle : ConfigBase
    {
        public static ConfigBattle Get(int id)
        {
            return GameConfigDatabase.Instance.Get<ConfigBattle>(id);
        }
        
        [InfoBox("引用指定战斗场景asset")]
        [LabelText("地图")] 
        public AssetReference MapScene;

        [LabelText("音乐")]
        public AssetReferenceT<AudioClip> Music; 
        

        /*[BoxGroup("战斗人物设置")] [LabelText("限制队友")] [SerializeReference]
        public List<String> LimitTeamMates;
        
        [BoxGroup("战斗人物设置")] [LabelText("增加队友")] [SerializeReference]
        public List<String> AutoTeamMates;
        
        [BoxGroup("战斗人物设置")] [LabelText("固定敌人")] [SerializeReference]
        public List<String> Enemies;*/
        
        
        public override async UniTask WarmUp()
        {
            
        }
    }
    
    [Serializable]
    public class SampleRate : IComparable<SampleRate>
    {
        [LabelText("样本")] 
        public String Sample;
        
        [LabelText("概率")]
        public int Rate;
        public int CompareTo(SampleRate obj)
        {
            return Rate.CompareTo(obj.Rate);
        }
        
    }
}