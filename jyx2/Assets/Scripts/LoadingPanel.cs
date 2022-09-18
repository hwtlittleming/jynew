
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;
using Jyx2;
using Jyx2.MOD;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{

    /// <summary>
    /// 载入场景
    /// </summary>
    /// <param name="sceneAsset">为null则返回主菜单</param>
    /// <returns></returns>
    public static async UniTask Create(AssetReference sceneAsset)
    {
        var loadingPanel = Jyx2ResourceHelper.CreatePrefabInstance("LoadingPanelCanvas").GetComponent<LoadingPanel>();
        GameObject.DontDestroyOnLoad(loadingPanel);
        await loadingPanel.LoadLevel(sceneAsset);
    }

    public Text m_LoadingText;

    private async UniTask LoadLevel(AssetReference sceneAsset)
    {
        UIManager.Instance.CloseAllUI();
        gameObject.SetActive(true);
        await UniTask.DelayFrame(1);
        await UniTask.WaitForEndOfFrame(); //否则BattleHelper还没有初始化
        
        //返回主菜单
        if (sceneAsset == null)
        {
            var handle = SceneManager.LoadSceneAsync(GameConst.DefaultMainMenuScene);
            while (!handle.isDone)
            {
                m_LoadingText.text = "载入中... ".GetContent(nameof(LoadingPanel)) + (int)(handle.progress * 100) + "%";
                await UniTask.WaitForEndOfFrame();
            }
        }
        //切换场景
        else
        {
            var async = Addressables.LoadSceneAsync(sceneAsset);
            while (!async.IsDone)
            {
                m_LoadingText.text = "载入中... ".GetContent(nameof(LoadingPanel)) + (int)(async.PercentComplete * 100) + "%";
                await UniTask.WaitForEndOfFrame();
            }
        }
        //要再等一帧
        await UniTask.WaitForEndOfFrame();
        
        Destroy(gameObject);
    }
}
