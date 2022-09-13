
using System.Collections;
using System.Collections.Generic;
using Animancer;
using Configs;
using UnityEngine;
using Jyx2;
using Jyx2Configs;


public class Jyx2SkillEditorEnemy : Jyx2AnimationBattleRole
{
    public int SkillId;
    
    Animator animator;

    override public Animator GetAnimator()
    {
        return animator;
    }
    
    // Start is called before the first frame update
    async void Start()
    {
        await BeforeSceneLoad.loadFinishTask;
        
        animator = GetComponent<Animator>();
        ConfigSkill skill = GameConfigDatabase.Instance.Get<ConfigSkill>(SkillId.ToString());
        this.CurDisplay = skill.Display;

        this.Idle();
    }

}
