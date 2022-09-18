
using Jyx2;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public partial class BattleActionOrderPanel:UIBase
{

    private ChildGoComponent childMgrComponent;

    List<RoleInstance> currentRoleList;
    float itemWidth = 0.0f;
    protected override void OnCreate()
    {
        InitTrans();
        childMgrComponent = GameUtil.GetOrAddComponent<ChildGoComponent>(MainRoot_RectTransform);
        childMgrComponent.Init(HeadItem_RectTransform);
        itemWidth = HeadItem_RectTransform.rect.width + 10;
    }

    protected override void OnShowPanel(params object[] allParams)
    {
        base.OnShowPanel(allParams);
        currentRoleList = allParams[0] as List<RoleInstance>;
        RefreshView();
    }

    void RefreshView() 
    {
        int childCount = currentRoleList != null ? currentRoleList.Count : 0;
        childMgrComponent.RefreshChildCount(childCount);
        if (childCount <= 0)
            return;
        RefreshChild();
    }

    void SetItemPos(Transform itemTrans, int index) 
    {
        float posX = itemWidth * index;
        itemTrans.localPosition = new Vector3(posX, 0, 0);
    }

    void RefreshChild() 
    {
        List<Transform> transList = childMgrComponent.GetUsingTransList();

        Transform itemTrans;
        for (int i = 0; i < transList.Count; i++)
        {
            itemTrans = transList[i];
            RoleInstance role = currentRoleList[i];
            if (role == null)
                continue;
            Image icon = itemTrans.Find("Mask/MainIcon").GetComponent<Image>();
            Text qingong = itemTrans.Find("Qingong").GetComponent<Text>();
            
            icon.LoadAsyncForget(role.configData.GetPic());
            
            qingong.text = string.Format($"轻功:{role.Speed}");

            Vector3 scale = i==0 ? new Vector3(1.2f,1.2f,1.0f):Vector3.one;
            itemTrans.localScale = scale;
            SetItemPos(itemTrans, i);
        }
    }

    protected override void OnHidePanel()
    {
        base.OnHidePanel();
    }
}
