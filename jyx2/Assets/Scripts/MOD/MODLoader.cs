
// - 配置表的载入
// - 场景、战斗场景载入
// - lua的载入
// - UI的载入
// - 其他相关资源的载入
// - MOD配置相关UI界面
// - 各种MODSample

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Jyx2.Middleware;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Jyx2.MOD
{
    //实际为ResLoader
    public static class MODLoader
    {

        #region 加载资源的接口,可复合MOD
        public static async UniTask<T> LoadAsset<T>(string uri) where T : Object
        {
            return await Addressables.LoadAssetAsync<T>(uri);
        }

        public static async UniTask<List<T>> LoadAssets<T>(List<string> uris) where T : Object
        {
            var allAssets = await Addressables.LoadAssetsAsync<T>(uris, null, Addressables.MergeMode.Union);
            return allAssets.ToList();
        }
#endregion

        //获取路径下所有asset文件 可做再获取文件夹下的
        public static List<string> getSonFiles(string path)
        {
            List<String> overridePaths = new List<String>();
            
            var paths = Directory.GetFiles(path).ToList();
            
            foreach (var p in paths)
            {
                if (p.EndsWith(".asset") || p.EndsWith(".lua")) //筛选文件
                {
                    overridePaths.Add(p.Replace("\\","/"));
                }
            }
            //若还有子目录 递归调用
            DirectoryInfo dir = new DirectoryInfo(path);
            DirectoryInfo[] dii = dir.GetDirectories();
            foreach (DirectoryInfo d in dii)  
            {  
                overridePaths.AddRange(getSonFiles( path +"/" + d.Name));;  
            } 
            return overridePaths;
        }
    }
}
