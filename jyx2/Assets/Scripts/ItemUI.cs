

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;


using Jyx2;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image m_Image;
    public Text m_NameText;
    public Text m_CountText;

    private const string ITEM_UI_PREFAB = "Jyx2ItemUI";
    
    public static ItemUI Create(ItemInstance item)
    {
        var prefab = Jyx2ResourceHelper.GetCachedPrefab(ITEM_UI_PREFAB);
        var obj = Instantiate(prefab); 
        var itemUI = obj.GetComponent<ItemUI>();
        itemUI.Show(item).Forget();
        return itemUI;
    }

    public String _id; //物品实例Id
    
    private async UniTaskVoid Show(ItemInstance item)
    {
        _id = item.Id;
        var color = ColorStringDefine.Default; //todo 按品质变色
            /*(int)item.ItemType == 2
                ? (int)item.NeedMPType == 2 ? ColorStringDefine.Default :
                (int)item.NeedMPType == 1 ? ColorStringDefine.Mp_type1 : ColorStringDefine.Mp_type0
                : ColorStringDefine.Default;*/
        
        m_NameText.text = $"<color={color}>{item.Name}</color>";
        m_CountText.text = item.Count.ToString();
        
        m_Image.LoadAsyncForget(item.GetPic());

    }

    public void Select(bool active) 
    {
        Transform select = transform.Find("Select");
        select.gameObject.SetActive(active);
    }

}
