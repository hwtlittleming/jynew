

using Jyx2;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using i18n.TranslatorDef;
using Jyx2Configs;
using UnityEngine;

public class UIHelper
{
    /// 获取物品的效果 //ChangeMPType
    public static Dictionary<int, int> GetItemEffect(ItemInstance item) 
    {
        Dictionary<int, int> result = new Dictionary<int, int>();
        if (item.AddHp != 0)//加血
            result.Add(13, item.AddHp);
        if (item.AddMaxHp != 0)//最大血量
            result.Add(16, item.AddMaxHp);
        if (item.AddMp != 0)//加内力
            result.Add(15, item.AddMp);
        if (item.AddMaxMp != 0)//最大内力
            result.Add(17, item.AddMaxMp);
        if (item.Attack != 0)//攻击
            result.Add(18, item.Attack);
        if (item.Speed != 0)//轻功
            result.Add(3, item.Speed);
        if (item.Defence != 0)//防御
            result.Add(4, item.Defence);
        if (item.Heal != 0)//医疗
            result.Add(6, item.Heal);

        return result;
    }

    /// <summary>
    /// 获取使用物品的需求 //NeedMPType; 
    /// </summary>
    /// <param name="item"></param>
    public static Dictionary<int, int> GetUseItemRequire(ItemInstance item) 
    {
        Dictionary<int, int> result = new Dictionary<int, int>();
        if (item.ConditionIQ > 0)
            result.Add(15, item.ConditionIQ);
        if (item.ConditionStrength > 0)
            result.Add(18, item.ConditionStrength);
        if (item.ConditionAgile > 0)
            result.Add(3, item.ConditionAgile);
        if (item.ConditionIQ > 0)
            result.Add(25, item.ConditionIQ);

        return result;
    }

    //使用人
    static string GetItemUser(ItemInstance item)
    {
        StringBuilder sb = new StringBuilder();

        RoleInstance user = GameRuntimeData.Instance.GetRoleInTeam(GameRuntimeData.Instance.GetItemUser(item));
        if (user != null)
        {
            sb.Append($"{user.Name}\n");
        }

        return sb.ToString();
    }

    //效果
    static string GetEffectText(ItemInstance item)
    {
        Dictionary<int, int> effects = UIHelper.GetItemEffect(item);
        StringBuilder sb = new StringBuilder();
        foreach (var effect in effects)
        {
            if (!GameConst.ProItemDic.ContainsKey(effect.Key.ToString()))
                continue;
            PropertyItem pro = GameConst.ProItemDic[effect.Key.ToString()];
            string valueText = effect.Value > 0 ? $"+{effect.Value}" : effect.Value.ToString();
            sb.Append($"{pro.Name}:  {valueText}\n");
        }
        return sb.ToString();
    }

    //使用要求
    static string GetUseRquire(ItemInstance item)
    {
        Dictionary<int, int> effects = UIHelper.GetUseItemRequire(item);
        StringBuilder sb = new StringBuilder();

        foreach (var effect in effects)
        {
            if (!GameConst.ProItemDic.ContainsKey(effect.Key.ToString()))
                continue;
            PropertyItem pro = GameConst.ProItemDic[effect.Key.ToString()];
            sb.Append($"{pro.Name}:  {effect.Value.ToString()}\n");
        }
        return sb.ToString();
    }

    public static string GetItemDesText(ItemInstance item)
    {
        StringBuilder strBuilder = new StringBuilder();
        strBuilder.Append($"<size=35><color=#FFDB00>{item.Name}</color></size>\n");
        strBuilder.Append($"{item.Desc}");

        string user = GetItemUser(item);
        if (!string.IsNullOrEmpty(user))
        {
            strBuilder.Append($"\n\n");
            strBuilder.Append("<size=28><color=#FFDB00>使用人</color></size>\n");
            strBuilder.Append(user);
        }
        string effect = GetEffectText(item);
        if (!string.IsNullOrEmpty(effect))
        {
            strBuilder.Append($"\n\n");
            //---------------------------------------------------------------------------
            //strBuilder.Append("<size=28><color=#FFDB00>效果</color></size>\n");
            //---------------------------------------------------------------------------
            //特定位置的翻译【MainMenu右下角当前版本的翻译】
            //---------------------------------------------------------------------------
            strBuilder.Append("<size=28><color=#FFDB00>效果</color></size>\n".GetContent(nameof(UIHelper)));
            //---------------------------------------------------------------------------
            //---------------------------------------------------------------------------
            strBuilder.Append(effect);
        }

        string useRequire = GetUseRquire(item);
        if (!string.IsNullOrEmpty(useRequire))
        {
            strBuilder.Append($"\n\n");
            strBuilder.Append("<size=28><color=#FFDB00>使用需求</color></size>\n");
            strBuilder.Append(useRequire);
        }

        return strBuilder.ToString();
    }
}
 