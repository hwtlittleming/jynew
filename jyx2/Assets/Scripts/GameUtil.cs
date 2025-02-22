
using Jyx2;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// JYX工具类
public class GameUtil
{
    /// <summary>
    /// 选择角色
    /// </summary>
    /// <param name="roles"></param>
    /// <param name="callback">如果放弃，则返回null</param>
    public static async UniTask SelectRole(IEnumerable<RoleInstance> roles, Action<RoleInstance> callback)
    {
        //选择使用物品的人
        List<string> selectionContent = new List<string>();
        foreach (var role in roles)
        {
            selectionContent.Add(role.Name);
        }
        selectionContent.Add("取消");
        var storyEngine = StoryEngine.Instance;
        storyEngine.BlockPlayerControl = true;
        
        SelectRoleParams selectParams = new SelectRoleParams();
        selectParams.roleList = roles.ToList();
        selectParams.title = "选择使用的人";
        selectParams.isDefaultSelect=false;
        selectParams.callback = (cbParam) => 
        {
            storyEngine.BlockPlayerControl = false;
            if (cbParam.isCancelClick == true)
            {
                return;
            }
            if (cbParam.selectList.Count <= 0)
            {
                callback(null);
                return;
            }
            var selectRole = cbParam.selectList[0];//默认只会选择一个
            callback(selectRole);
        };

        await UIManager.Instance.ShowUIAsync(nameof(SelectRolePanel), selectParams);
    }
    
    /// 显示冒泡文字
    public static void DisplayPopinfo(string msg, float duration =2f)
    {
        StoryEngine.Instance.DisplayPopInfo(msg, duration);
    }

    public static async void ShowFullSuggest(string content, string title = "", Action cb = null) 
    {
        await UIManager.Instance.ShowUIAsync(nameof(FullSuggestUIPanel), content, title, cb);
    }

    public static void GamePause(bool pause) 
    {
        if (pause)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    public static Component GetOrAddComponent(Transform trans,string type)
    {
        Component com = trans.GetComponent(type);
        if (com == null) 
        {
            System.Type t = System.Type.GetType(type);
            com = trans.gameObject.AddComponent(t);
        }
        return com;
    }

    public static T GetOrAddComponent<T>(Transform trans) where T:Component 
    {
        T com = trans.GetComponent<T>();
        if (com == null)
        {
            com = trans.gameObject.AddComponent<T>();
        }
        return com;
    }
    
    public static void LogError(string str) 
    {
        Debug.LogError(str);
    }
    
    public static void CallWithDelay(double time,Action action)
    {
        if(time == 0)
        {
            action();
            return;
        }

        Observable.Timer(TimeSpan.FromSeconds(time)).Subscribe(ms =>
        {
            action();
        });
    }
    

    private static void ChangeScence()
    {
        //惨叫
        string path = "Assets/BuildSource/sound/nancanjiao.wav";
        if (Camera.main != null) AudioManager.PlayClipAtPoint(path, Camera.main.transform.position).Forget();

        //血色
        var blackCover = LevelMaster.Instance.transform.Find("UI/BlackCover");
        if (blackCover == null)
        {
            Debug.LogError("DarkScence error，找不到LevelMaster/UI/BlackCover");
            return;
        }

        blackCover.gameObject.SetActive(true);
        var img = blackCover.GetComponent<Image>();
        img.DOColor(Color.red, 2).OnComplete(() =>
        {
            blackCover.gameObject.SetActive(false);
        });
    }
    
}
