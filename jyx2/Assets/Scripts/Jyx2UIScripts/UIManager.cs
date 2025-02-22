

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;
using Jyx2.MOD;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum UILayer 
{
    MainUI = 0,//主界面层
    NormalUI = 1,//普通界面层
    PopupUI = 2,//弹出层
    Top = 3,//top层 高于弹出层
}

public class UIManager : MonoBehaviour
{
    static UIManager _instace;
    public static UIManager Instance 
    {
        get 
        {
            if (_instace == null) 
            {
                var prefab = Resources.Load<GameObject>("MainCanvas");
                var obj = Instantiate(prefab);
                obj.gameObject.name = "MainCanvas";
                _instace = obj.GetComponent<UIManager>();
                _instace.Init();
                DontDestroyOnLoad(_instace);
            }
            return _instace;
        }
    }

    private Transform m_mainParent;
    private Transform m_normalParent;
    private Transform m_popParent;
    private Transform m_topParent;

    private Dictionary<string, UIBase> m_uiDic = new Dictionary<string, UIBase>();
    private UIBase m_currentMainUI;
    private Stack<UIBase> m_normalUIStack = new Stack<UIBase>();
    private Stack<UIBase> m_PopUIStack = new Stack<UIBase>();

    void Init()
    {
        m_mainParent = transform.Find("MainUI");
        m_normalParent = transform.Find("NormalUI");
        m_popParent = transform.Find("PopupUI");
        m_topParent = transform.Find("Top");
    }

    public bool IsTopVisibleUI(UIBase ui)
	{
        if (!ui.gameObject.activeSelf)
            return false;

        if (ui.Layer == UILayer.MainUI)
		{
			//make sure no normal and popup ui on top
			return noShowingNormalUi() &&
				(noInterferingPopupUI());
		}
		else if (ui.Layer == UILayer.NormalUI)
		{
            UIBase currentUi = m_normalUIStack.Count > 0 ? m_normalUIStack.Peek() : null;
            if (currentUi == null)
                return true;
            
			return (ui == currentUi || ui.transform.IsChildOf(currentUi.transform)) && noInterferingPopupUI();
		}
        else if (ui.Layer == UILayer.PopupUI)
		{
            return (m_PopUIStack.Count > 0 ? m_PopUIStack.Peek() : null) == ui;
		}
        else if (ui.Layer == UILayer.Top)
		{
            return true;
		}

        return false;
	}

	private bool noShowingNormalUi()
	{
        return !m_normalUIStack
            .Any(ui => ui.gameObject.activeSelf);
	}

	private bool noInterferingPopupUI()
	{
        //common tips panel has no interaction, doesn't count towards active uis
        return !m_normalUIStack
            .Any(ui => ui.gameObject.activeSelf) || (m_PopUIStack.All(p => p is CommonTipsUIPanel));
	}

	public async void GameStart()
    {
        await ShowUIAsync(nameof(GameMainMenu));
        //---------------------------------------------------------------------------
        //await ShowUIAsync(nameof(GameInfoPanel),$"当前版本：{Application.version}");
        //---------------------------------------------------------------------------
        //特定位置的翻译【MainMenu右下角当前版本的翻译】
        //---------------------------------------------------------------------------
        await ShowUIAsync(nameof(GameInfoPanel), string.Format("当前版本：{0}".GetContent(nameof(UIManager)), Application.version));
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        GraphicSetting.GlobalSetting.Execute();
    }

    Transform GetUIParent(UILayer layer) 
    {
        switch (layer) 
        {
            case UILayer.MainUI:
                return m_mainParent;
            case UILayer.NormalUI:
                return m_normalParent;
            case UILayer.PopupUI:
                return m_popParent;
            case UILayer.Top:
                return m_topParent;
            default:
                return transform;
        }
    }

    Dictionary<string, object[]> _loadingUIParams = new Dictionary<string, object[]>();
    public void ShowUI(string uiName,params object[] allParams) 
    {
        UIBase uibase;
        if (m_uiDic.ContainsKey(uiName))
        {
            uibase = m_uiDic[uiName];
            if (uibase.IsOnly)//如果这个层唯一存在 那么先关闭其他
                PopAllUI(uibase.Layer);
            PushUI(uibase);
            uibase.Show(allParams);
        }
        else
        {
            if (_loadingUIParams.ContainsKey(uiName)) //如果正在加载这个UI 那么覆盖参数
            {
                _loadingUIParams[uiName] = allParams;
                return;
            }

            _loadingUIParams[uiName] = allParams;
            string uiPath = string.Format(GameConst.UI_PREFAB_PATH, uiName);

            Addressables.InstantiateAsync(uiPath).Completed += r => { OnUILoaded(r.Result); };
        }
    }

    public async UniTask ShowUIAsync(string uiName, params object[] allParams)
    {
        UIBase uibase;
        if (m_uiDic.ContainsKey(uiName))
        {
            uibase = m_uiDic[uiName];
            if (uibase.IsOnly)//如果这个层唯一存在 那么先关闭其他
                PopAllUI(uibase.Layer);
            PushUI(uibase);
            uibase.Show(allParams);
        }
        else
        {
            if (_loadingUIParams.ContainsKey(uiName)) //如果正在加载这个UI 那么覆盖参数
            {
                _loadingUIParams[uiName] = allParams;
                return;
            }

            _loadingUIParams[uiName] = allParams;
            string uiPath = string.Format(GameConst.UI_PREFAB_PATH, uiName);

            var prefab = await MODLoader.LoadAsset<GameObject>(uiPath);
            var go = Instantiate(prefab);
            OnUILoaded(go);
        }
    }

    //UI加载完后的回调
    void OnUILoaded(GameObject go) 
    {
        string uiName = go.name.Replace("(Clone)", "");
        object[] allParams = _loadingUIParams[uiName];
        Component com = GameUtil.GetOrAddComponent(go.transform, uiName);
        
        UIBase uibase = com as UIBase;
        Transform parent = GetUIParent(uibase.Layer);
        go.transform.SetParent(parent);

		//听取ui的 OnVisibilityToggle event
		uibase.VisibilityToggled += Uibase_OnVisibilityToggle;

        uibase.Init();
        m_uiDic[uiName] = uibase;
        if (uibase.IsOnly)//如果这个层唯一存在 那么先关闭其他
            PopAllUI(uibase.Layer);
        PushUI(uibase);

        uibase.Show(allParams);
        _loadingUIParams.Remove(uiName);
    }

	private void Uibase_OnVisibilityToggle(UIBase ui, bool obj)
	{
		UIVisibilityToggled?.Invoke(ui, obj);
	}

    public event Action<UIBase, bool> UIVisibilityToggled;

	//显示主界面 LoadingPanel中加载完场景调用 移到这里来 方便修改
	public async UniTask ShowMainUI()
    {
        var map = LevelMaster.GetCurrentGameMap();
        if (map != null && map.Tags.Contains("BATTLE"))
        {
            await ShowUIAsync(nameof(BattleMainUIPanel), BattleMainUIState.None);
            return;
        }
        else
            await ShowUIAsync(nameof(MainUIPanel));
    }

    void PushUI(UIBase uibase) 
    {
        switch (uibase.Layer)
        {
            case UILayer.MainUI:
                if (m_currentMainUI && m_currentMainUI != uibase)
                {
                    m_currentMainUI.Hide();
                }
                m_currentMainUI = uibase;
                break;
            case UILayer.NormalUI:
                m_normalUIStack.Push(uibase);
                break;
            case UILayer.PopupUI:
                m_PopUIStack.Push(uibase);
                break;
        }
    }

    void PopAllUI(UILayer layer) 
    {
        if (layer == UILayer.NormalUI)
        {
            PopUI(null, m_normalUIStack);
        }
        else if (layer == UILayer.PopupUI) 
        {
            PopUI(null, m_PopUIStack);
        }
    }

    void PopUI(UIBase ui, Stack<UIBase> uiStack) 
    {
        if (!uiStack.Contains(ui))
            return;
        UIBase node = uiStack.Pop();
        while (node) 
        {
            if (node == ui) 
            {
                node.Hide();
                return;
            }
            if (uiStack.Count <= 0)
                return;
            node.Hide();
            node = uiStack.Pop();
        }
    }

    public void HideUI(string uiName) 
    {
        if (!m_uiDic.ContainsKey(uiName))
            return;
        UIBase uibase = m_uiDic[uiName];
        if (m_normalUIStack.Contains(uibase))
        {
            PopUI(uibase, m_normalUIStack);
        }
        else if (m_PopUIStack.Contains(uibase))
        {
            PopUI(uibase, m_PopUIStack);
        }
        else if (uibase.Layer == UILayer.MainUI)
            uibase.Hide();
        else
            uibase.Hide();
    }

    public void SetMainUIActive(bool active) 
    {
        if (m_currentMainUI == null)
            return;
        if (active)
            m_currentMainUI.Show();
        else
            m_currentMainUI.Hide();
    }

    public Camera GetUICamera() 
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas.worldCamera)
            return canvas.worldCamera;
        return Camera.main;
    }

    //关闭所有的UI
    public void CloseAllUI() 
    {
        foreach (var item in m_uiDic)
        {
            HideUI(item.Key);
        }
    }
}
