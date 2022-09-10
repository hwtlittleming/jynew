using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Jyx2.MOD;
using Jyx2.Middleware;

namespace Jyx2Configs
{
    //用于存储和加载 静态数据  类继承ScriptableObject后，可将类的一些不常变的实例作为数据 存储于asset文件中供取用 
    //类的属性值 在游戏运行期间改变后 不需要保存下来的数据 就可以考虑用此方法
    public class GameConfigDatabase 
    {
        #region Singleton
        public static GameConfigDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameConfigDatabase();
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        private static GameConfigDatabase _instance;

        private GameConfigDatabase()
        {
        }
    #endregion

        private readonly Dictionary<Type, Dictionary<int, Jyx2ConfigBase>> _dataBase =
            new Dictionary<Type, Dictionary<int, Jyx2ConfigBase>>();

        private bool _isInited = false;
        
        public async UniTask Init()
        {
            if (_isInited)
                return;
            
            _isInited = true;
            int total = 0;
            total += await Init<Jyx2ConfigCharacter>("Assets/BuildSource/Configs/Characters");
            total += await Init<Jyx2ConfigItem>("Assets/BuildSource/Configs/Items");
            total += await Init<Jyx2ConfigSkill>("Assets/BuildSource/Configs/Skills");
            total += await Init<Jyx2ConfigShop>("Assets/BuildSource/Configs/Shops");
            total += await Init<Jyx2ConfigMap>("Assets/BuildSource/Configs/Maps");
            total += await Init<Jyx2ConfigBattle>("Assets/BuildSource/Configs/Battles");
            
            Debug.Log($"载入完成，总数{total}个配置asset");
        }

        public T Get<T>(int id) where T : Jyx2ConfigBase
        {
            var type = typeof(T);
            if (_dataBase.TryGetValue(type, out var db))
            {
                if (db.TryGetValue(id, out var asset))
                {
                    return (T)asset;
                }
            }
            return null;
        }
        public T Get<T>(string id) where T : Jyx2ConfigBase
        {
            return Get<T>(int.Parse(id));
        }

        public bool Has<T>(string id) where T : Jyx2ConfigBase
        {
            return Get<T>(id) != null;
        }
        
        public IEnumerable<T> GetAll<T>() where T : Jyx2ConfigBase
        {
            var type = typeof(T);
            if (_dataBase.TryGetValue(type, out var db))
            {
                foreach (var v in db.Values)
                {
                    yield return (T) v;
                }
            }
        }
        
        /// 初始化指定类型配置
        public async UniTask<int> Init<T>(string path) where T : Jyx2ConfigBase
        {
            if (_dataBase.ContainsKey(typeof(T)))
            {
                throw new Exception("类型" + typeof(T) + "已经创建过了，不允许重复创建！");
            }
            
            var overridePaths = MODLoader.getSonFiles(path);
            
            var assets = await MODLoader.LoadAssets<T>(overridePaths);

            var db = new Dictionary<int, Jyx2ConfigBase>();
            _dataBase[typeof(T)] = db;
            foreach (var asset in assets)
            {
                if (db.ContainsKey(asset.Id))
                {
                    Debug.Log($"ID重复，覆盖写入: {asset.Name}-->{db[asset.Id].Name}");
                }
                db[asset.Id] = asset;
                asset.WarmUp().Forget();
            }

            return db.Count;
        }
    }
    
    
}
