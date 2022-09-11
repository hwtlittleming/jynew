

using System;
using Cysharp.Threading.Tasks;


using Jyx2;
using Jyx2Configs;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jyx2
{
    public class LevelLoader
    {
        //加载地图
        public static void LoadGameMap(Jyx2ConfigMap map, LevelMaster.LevelLoadPara para = null, Action callback = null)
        {
            LevelMaster.loadPara = para != null ? para : new LevelMaster.LevelLoadPara(); //默认生成一份

            DoLoad(map, callback).Forget();
        }

        static async UniTask DoLoad(Jyx2ConfigMap map, Action callback)
        {
            LevelMaster.SetCurrentMap(map);
            await LoadingPanel.Create(map.MapScene);
            callback?.Invoke();
        }
        
        //加载战斗
        public static void LoadBattle(int battleId, Action<BattleResult> callback)
        {
            var battle = GameConfigDatabase.Instance.Get<Jyx2ConfigBattle>(battleId);
            if (battle == null)
            {
                Debug.LogError($"战斗id={battleId}未定义");
                return;
            }
            
            DoloadBattle(battle, callback).Forget();
        }

        private static async UniTask DoloadBattle(Jyx2ConfigBattle battle, Action<BattleResult> callback)
        {
            var formalMusic = AudioManager.GetCurrentMusic(); //记住当前的音乐，战斗后还原

            LevelMaster.IsInBattle = true;
            await LoadingPanel.Create(battle.MapScene);
                
            GameObject obj = new GameObject("BattleLoader");
            var battleLoader = obj.AddComponent<BattleLoader>();
            battleLoader.m_BattleId = battle.Id;
                
            //播放之前的地图音乐
            battleLoader.Callback = (rst) =>
            {
                LevelMaster.IsInBattle = false;
                AudioManager.PlayMusic(formalMusic);
                callback(rst);
            };
        }
        
        
    }
}
