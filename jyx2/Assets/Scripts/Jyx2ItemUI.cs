

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;


using Jyx2;
using Jyx2Configs;
using UnityEngine;
using UnityEngine.UI;

public class Jyx2ItemUI : MonoBehaviour
{
    public Image m_Image;
    public Text m_NameText;
    public Text m_CountText;

    private const string ITEM_UI_PREFAB = "Jyx2ItemUI";
    
    public static Jyx2ItemUI Create(String id,int count)
    {
        var prefab = Jyx2ResourceHelper.GetCachedPrefab(ITEM_UI_PREFAB);
        var obj = Instantiate(prefab); 
        var itemUI = obj.GetComponent<Jyx2ItemUI>();
        itemUI.Show(id, count).Forget();
        return itemUI;
    }

    private String _id;

    // 获取主角携带的物品
    public ItemInstance GetItem()
    {
        return   GameRuntimeData.Instance.AllRoles[0].GetItem(_id);  //GameConfigDatabase.Instance.Get<Jyx2ConfigItem>(_id);
    }

    private async UniTaskVoid Show(String id,int count)
    {
        _id = id;
        var item = GetItem();//0-阴性内力，1-阳性内力，2-中性内力
        var color = ColorStringDefine.Default;
            /*(int)item.ItemType == 2
                ? (int)item.NeedMPType == 2 ? ColorStringDefine.Default :
                (int)item.NeedMPType == 1 ? ColorStringDefine.Mp_type1 : ColorStringDefine.Mp_type0
                : ColorStringDefine.Default;*/
        
        m_NameText.text = $"<color={color}>{item.Name}</color>";
        m_CountText.text = (count > 1 ? count.ToString() : "");
        
        m_Image.LoadAsyncForget(item.GetPic());

    }

    public void Select(bool active) 
    {
        Transform select = transform.Find("Select");
        select.gameObject.SetActive(active);
    }

}
