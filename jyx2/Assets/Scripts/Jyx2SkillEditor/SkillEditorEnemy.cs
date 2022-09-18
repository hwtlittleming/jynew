
using Configs;
using UnityEngine;
using Jyx2;

public class SkillEditorEnemy : AnimationBattleRole
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
