

using System;
using Jyx2;
using System.Collections;
using System.Collections.Generic;
using i18n.TranslatorDef;
using UnityEngine;
using UnityEngine.UI;

public enum BattleMainUIState 
{
    None = 0,
    ShowRole = 1,//显示角色
    ShowHUD = 2,//显示血条
}

public partial class BattleMainUIPanel:UIBase
{
    public override UILayer Layer => UILayer.MainUI;

    ChildGoComponent childMgr;
    RoleInstance m_currentRole;
    protected override void OnCreate()
    {
        InitTrans();
        childMgr = GameUtil.GetOrAddComponent<ChildGoComponent>(BattleHpRoot_RectTransform);
        childMgr.Init(HUDItem_RectTransform, OnHUDCreate);
    }

    protected override void OnShowPanel(params object[] allParams)
    {
        base.OnShowPanel(allParams);
        
        if(childMgr==null){
            childMgr = GameUtil.GetOrAddComponent<ChildGoComponent>(BattleHpRoot_RectTransform);
            childMgr.Init(HUDItem_RectTransform, OnHUDCreate);
        }
        BattleMainUIState state = (BattleMainUIState)allParams[0];
        if (state == BattleMainUIState.ShowRole)
        {
            m_currentRole = allParams[1] as RoleInstance;
            ShowRole();
        }
        else if (state == BattleMainUIState.ShowHUD)
        {
            ShowHUDSlider();
        }else
            ShowRole();
    }

    void ShowRole() 
    {
        if (m_currentRole == null)
        {
            CurrentRole_RectTransform.gameObject.SetActive(false);
            return;
        }
        CurrentRole_RectTransform.gameObject.SetActive(true);
        NameText_Text.text = m_currentRole.Name;
        var color1 = m_currentRole.GetHPColor1();
        var color2 = m_currentRole.GetHPColor2();
        var color = m_currentRole.GetMPColor();

        DetailText_Text.text = (string.Format(
            "生命 <color={0}>{1}</color>/<color={2}>{3}</color>\n内力 <color={4}>{5}/{6}</color>".GetContent(nameof(BattleMainUIPanel)),
             color1, m_currentRole.Hp, color2, m_currentRole.MaxHp, color, m_currentRole.Mp,
            m_currentRole.MaxMp));

        PreImage_Image.LoadAsyncForget(m_currentRole.configData.GetPic());
    }
    
	public override void Update()
	{
        //battle action ui handles update by itself, this is calling it twice

        //BattleActionUIPanel panel = FindObjectOfType<BattleActionUIPanel>();
        //if (panel != null)
        //    panel.Update();
	}

	void OnHUDCreate(Transform hudTrans) 
    {
        HUDItem item = GameUtil.GetOrAddComponent<HUDItem>(hudTrans);
        item.Init();
    }

    //显示血条
    void ShowHUDSlider() 
    {
        List<RoleInstance> roles = BattleManager.Instance.GetModel().AliveRoles;
        childMgr.RefreshChildCount(roles.Count);
        List<Transform> childTrans = childMgr.GetUsingTransList();
        for (int i = 0; i < childTrans.Count; i++)
        {
            HUDItem item = GameUtil.GetOrAddComponent<HUDItem>(childTrans[i]);
            RoleInstance role = roles[i];
            if (role == null)
                continue;
            item.BindRole(role);
        }
    }
    
    protected override void OnHidePanel()
    {
        base.OnHidePanel();
        childMgr=null;
        m_currentRole=null;
    }
}
