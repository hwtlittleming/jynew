


using Jyx2;
using System;
using System.IO;
using System.Threading.Tasks;
using Configs;
using Cysharp.Threading.Tasks;
using Jyx2.MOD;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Jyx2
{
    public static class ImageLoadHelper
    {
        public static void LoadAsyncForget(this Image image, UniTask<Sprite> task)
        {   
            LoadAsync(image,task).Forget();
        }
        
        public static async UniTask LoadAsync(this Image image, UniTask<Sprite> task)
        {
            image.gameObject.SetActive(false);
            image.sprite = await task;
            image.gameObject.SetActive(true);
        }
        
    }
}

public static class Jyx2ResourceHelper
{
    private static bool _isInited = false;
    
    public static async Task Init()
    {
        //已经初始化过了
        if (_isInited)
        {
            return;
        }
        _isInited = true;
        //全局配置表
        var t = await MODLoader.LoadAsset<GlobalAssetConfig>("Assets/BuildSource/Configs/GlobalAssetConfig.asset");
        if (t != null)
        {
            GlobalAssetConfig.Instance = t;
            t.OnLoad();
        }

        //技能池
        /*var overridePaths = MODLoader.getSonFiles("Assets/BuildSource/Configs/SkillDisplays");
        var task = await MODLoader.LoadAssets<Jyx2SkillDisplayAsset>(overridePaths);
        if (task != null)
        {
            Jyx2SkillDisplayAsset.All = task;
        }*/

        //基础配置表
        await GameConfigDatabase.Instance.Init();

        /*从excel取配置数据的方法
         #if UNITY_EDITOR
                //excel转data二进制
                string dataPath = "Assets/BuildSource/Configs/Datas.bytes";
                if (File.Exists(dataPath))
                {
                    File.Delete(dataPath);
                }
                ExcelTools.GenerateConfigsFromExcel<Jyx2ConfigBase>( "Assets/BuildSource/Configs");
                AssetDatabase.Refresh();
                var config = await MODLoader.LoadAsset<TextAsset>($"Assets/Configs/Datas.bytes");
                GameConfigDatabase.Instance._dataBase = ExcelTools.ProtobufDeserialize<Dictionary<Type, Dictionary<int, Jyx2ConfigBase>>>(config.bytes);
        #endif  */
        
        //lua
        await LuaManager.InitLuaMapper();
        LuaManager.Init();
    }

    public static GameObject GetCachedPrefab(string path)
    {
        if(GlobalAssetConfig.Instance.CachePrefabDict.TryGetValue(path, out var prefab))
        {
            return prefab;
        }
        
        Debug.LogError($"载入缓存的Prefab失败：{path}(是否没填入GlobalAssetConfig.CachedPrefabs?)");
        return null;
    }

    public static GameObject CreatePrefabInstance(string path)
    {
        var obj = GetCachedPrefab(path);
        return Object.Instantiate(obj);
    }

    public static void ReleasePrefabInstance(GameObject obj)
    {
        Object.Destroy(obj);
    }

    [Obsolete("待修改为tilemap")]
    public static async UniTask<SceneCoordDataSet> GetSceneCoordDataSet(string sceneName)
    {
        string path = $"{ConStr.BattleBlockDatasetPath}{sceneName}_coord_dataset.bytes";
        var result = await MODLoader.LoadAsset<TextAsset>(path);
        using var memory = new MemoryStream(result.bytes);
        return Serializer.Deserialize<SceneCoordDataSet>(memory);
    }

    public static async UniTask<Jyx2NodeGraph> LoadEventGraph(String id)
    {
        string url = $"Assets/BuildSource/EventsGraph/{id}.asset";
        var rst = await Addressables.LoadResourceLocationsAsync(url).Task;
        if (rst.Count == 0)
        {
            return null;
        }

        return await MODLoader.LoadAsset<Jyx2NodeGraph>(url);
    }
    
    //根据Addressable的Ref查找他实际存储的路径
    public static string GetAssetRefAddress(AssetReference reference, Type type)
    {
        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator.Locate(reference.AssetGUID, type, out var locs))
            {
                foreach (var loc in locs)
                {
                    return loc.ToString();
                }
            }
        }

        return "";
    }
}