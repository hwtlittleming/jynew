using System;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;
using Jyx2;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Configs
{
    [CreateAssetMenu(menuName = "配置文件/地图", fileName = "地图ID_地图名")]
    public class ConfigMap : ConfigBase
    {
        public static ConfigMap Get(int id)
        {
            
            return GameConfigDatabase.Instance.Get<ConfigMap>(id);
        }
        
        [InfoBox("引用指定场景asset")]
        [LabelText("地图")]
        public AssetReference MapScene;

        [LabelText("场景音乐")]
        public AssetReferenceT<AudioClip> InMusic;

        [InfoBox("0开局开启  1开局关闭")]
        [LabelText("进入条件")] 
        public int EnterCondition;

        [LabelText("标签")] 
        public string Tags;
        
        //陆地天空河流各一种战斗地图 或者 每个大城镇 每种小地形一种地图;
        [LabelText("地图类型")] 
        public String MapKind;
        
        public override async UniTask WarmUp()
        {
            _isWorldMap = Tags.Contains("WORLDMAP");
            _isNoNavAgent = Tags.Contains("NONAVAGENT");
        }
        
        public string GetShowName()
        {
            //特定位置的翻译【小地图左上角的主角居显示】
            if (GlobalAssetConfig.Instance.defaultHomeName.Equals(Name)) return GameRuntimeData.Instance.Player.Name + "居".GetContent(nameof(ConfigMap));
            return Name;
        }
        
        //获得开场地图
        public static ConfigMap GetGameStartMap()
        {
            foreach(var map in GameConfigDatabase.Instance.GetAll<ConfigMap>())
            {
                if (map.Tags.Contains("START"))
                {
                    return map;
                }
            }
            return null;
        }
        
        /// 是否是大地图
        public bool IsWorldMap() { return _isWorldMap;}
        private bool _isWorldMap;
        
        /// 是否不能寻路
        public bool IsNoNavAgent() { return _isNoNavAgent;}
        private bool _isNoNavAgent;
        
    }
}
