
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
    void InitTrans() 
    {
        if (hasInit)
            return;
        hasInit = true;
        m_icon = transform.Find("Icon").GetComponent<Image>();
        m_skillText = transform.Find("SkillText").GetComponent<Text>();
    }

    public void RefreshSkill(SkillInstance skill) 
    {
        InitTrans();
        m_currentSkill = skill;

        string skillText = $"{skill.Name}\nLv.{skill.Level}";
        m_skillText.text = skillText;
    }
    
    public SkillInstance GetSkill() 
    {
        return m_currentSkill;
    }
}
