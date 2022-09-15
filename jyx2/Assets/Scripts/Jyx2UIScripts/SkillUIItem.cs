
using Jyx2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIItem : MonoBehaviour
{
    public SkillInstance m_currentSkill;
    
    private bool hasInit = false;
    Image m_icon;
    Text m_skillText;
    Transform m_select;
    void InitTrans() 
    {
        if (hasInit)
            return;
        hasInit = true;
        m_icon = transform.Find("Icon").GetComponent<Image>();
        m_skillText = transform.Find("SkillText").GetComponent<Text>();
        m_select = transform.Find("CurrentTag");
    }

    public void RefreshSkill(SkillInstance skill) 
    {
        InitTrans();
        m_currentSkill = skill;

        string skillText = $"{skill.Name}\nLv.{skill.Level}";
        m_skillText.text = skillText;
    }

    public void SetSelect(bool se) 
    {
        m_select.gameObject.SetActive(se);
    }

    public SkillInstance GetSkill() 
    {
        return m_currentSkill;
    }
}
