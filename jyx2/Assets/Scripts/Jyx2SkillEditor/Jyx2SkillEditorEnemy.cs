
using System.Collections;
using System.Collections.Generic;
using Animancer;

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
        Jyx2ConfigSkill skill = GameConfigDatabase.Instance.Get<Jyx2ConfigSkill>(SkillId.ToString());
        this.CurDisplay = skill.Display;

        this.Idle();
    }

}
