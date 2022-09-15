
using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Cysharp.Threading.Tasks;

using Jyx2;
using UnityEngine;

public partial class SkillEditorUIPanel:Jyx2_UIBase
{
    
    public MapRole player;

    public Jyx2SkillEditorEnemy[] enemys;

    private int skillId;
    private int skillLevel;
    private int roleKey;

    private readonly List<ConfigSkill> allSkills = new List<ConfigSkill>();
    private readonly List<ConfigCharacter> allRole = new List<ConfigCharacter>();
	protected override void OnCreate()
    {
        InitTrans();
        skillId = 1;
        skillLevel = 1;
        roleKey = 0;
        
        dropSkillId_Dropdown.ClearOptions();
        dropSkillLevel_Dropdown.ClearOptions();
        dropModelId_Dropdown.ClearOptions();

        List<string> skills = new List<string>();
        List<string> levels = new List<string>();
        List<string> roles = new List<string>();
        foreach(var skill in GameConfigDatabase.Instance.GetAll<ConfigSkill>())
        {
            allSkills.Add(skill);
            skills.Add(skill.Name);
        }
        dropSkillId_Dropdown.AddOptions(skills);

        for(int i = 0; i < 10; ++i)
        {
            levels.Add((i + 1).ToString());
        }
        dropSkillLevel_Dropdown.AddOptions(levels);
        
        foreach(var role in GameConfigDatabase.Instance.GetAll<ConfigCharacter>())
        {
            allRole.Add(role);
            roles.Add(role.Name);
        }
        dropModelId_Dropdown.AddOptions(roles);
        
        BindListener(this.btnDisplaySkill_Button,OnDisplaySkill);
        BindListener(this.btnRunAnim_Button,OnRunAnim);
        BindListener(this.btnSwitchModel_Button,OnSwitchModel);
        this.dropSkillId_Dropdown.onValueChanged.RemoveAllListeners();
        this.dropSkillId_Dropdown.onValueChanged.AddListener(OnSwitchSkill);
        this.dropSkillLevel_Dropdown.onValueChanged.RemoveAllListeners();
        this.dropSkillLevel_Dropdown.onValueChanged.AddListener(OnSwitchSkillLevel);
        this.dropModelId_Dropdown.onValueChanged.RemoveAllListeners();
        this.dropModelId_Dropdown.onValueChanged.AddListener(OnSwitchdropModelId);
	}

    private void OnSwitchdropModelId(int index)
    {
        var role = allRole[index];
        roleKey = role.Id;
        //下面这一行会触发模型更新，这样显得按钮很没有用，所以我取消了
        //OnSwitchModel();
    }

    private void OnSwitchSkillLevel(int arg0)
    {
        skillLevel = arg0 + 1;
    }

    private void OnSwitchSkill(int index)
    {
        var skill = allSkills[index];
        skillId = Convert.ToInt32(skill.Id);
        SwitchSkillPose();
    }

    protected override void OnShowPanel(params object[] allParams)
    {
        base.OnShowPanel(allParams);
        player = allParams[0] as MapRole;
        enemys = allParams[1] as Jyx2SkillEditorEnemy[];
        //DoSwitchRoleModel().Forget();//这里也去掉，防止多次加载模型
    }

    private void OnSwitchModel()
    {
        DoSwitchRoleModel().Forget();
    }

    private void OnRunAnim()
    {
        DoSwitchMove();
    }

    private void OnDisplaySkill()
    {
        TryDisplaySkill().Forget();
    }

    async UniTask DoSwitchRoleModel()
    {
        var role = new RoleInstance(this.roleKey);
        await player.BindRoleInstance(role);
        await player.RefreshModel();//添加这一行刷新模型
        
        //不必要的指定
        //var animator = player.GetAnimator();
        //animator.runtimeAnimatorController = player.GetComponent<Animator>().runtimeAnimatorController; //force set animator
        
        //不必要切换姿势
        //SwitchSkillPose();
    }

    void DoSwitchMove()
    {
        Debug.Log("do switch move");
        player.Run();
    }

    async UniTask TryDisplaySkill()
    { 
        var wugong = new SkillInstance(skillId);

        SkillCastHelper helper = new SkillCastHelper();
        helper.Source = player;
        helper.Targets = enemys;
        
        wugong.Level = skillLevel;
        helper.Zhaoshi = wugong;

        //根据不同的技能覆盖类型，显示不同的效果
        Transform[] blocks = null;
        switch (wugong.SkillCoverType)
        {
            case 0:
                blocks = skillEditor.faceTrans;
                break;
            case 1:
                blocks = skillEditor.lineTrans;
                break;
            case 2:
                blocks = skillEditor.crossTrans;
                break;
            case 3:
                
                //任选一个敌人受击
                blocks = new Transform[1] {Jyx2.Middleware.Tools.GetRandomElement(enemys).transform};
                
                //直接在每个敌人身上受击
                /*blocks = new Transform[enemys.Length];
                int index = 0;
                foreach(var e in enemys)
                {
                    blocks[index++] = e.transform;
                }*/
                break;
            default:
                Debug.LogError("invalid skill cover type!");
                break;                
        }
        
        helper.CoverBlocks = blocks; 
        

        await helper.Play();
        if (skillEditor.TestZuoyouhubo) //测试左右互搏
        {
            await helper.Play();
        }
    }

    /// <summary>
    /// 切换技能待机动作
    /// </summary>
    void SwitchSkillPose()
    {
        var wugong = new SkillInstance(skillId);
        //切换武器和动作

        player.SwitchSkillTo(wugong);
    }

    public void SwitchToSkill(string skillName)
    {
        var skill = GameConfigDatabase.Instance.GetAll<ConfigSkill>().Single(p => p.Name.Equals(skillName));
        if (skill != null)
        {
            int index = allSkills.IndexOf(skill);
            if (index != -1)
            {
                dropSkillId_Dropdown.value = index;
                OnDisplaySkill();
            }
        }
    }

    private Jyx2SkillEditor skillEditor;
    void Start()
    {
        skillEditor = FindObjectOfType<Jyx2SkillEditor>();
    }
}
