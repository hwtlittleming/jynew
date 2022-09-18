
using Jyx2;
using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public partial class BattleActionUIPanel : UIBase
{
	public override UILayer Layer => UILayer.NormalUI;
	public RoleInstance GetCurrentRole()
	{
		return m_currentRole;
	}

	RoleInstance m_currentRole; //当前主角
	
	ChildGoComponent childMgr;
	
	private Action<BattleManager.ManualResult> callback;

	private Dictionary<SkillInstance,Button> skillList = new Dictionary<SkillInstance,Button>(); 
	private List<Button> greyButtons;//置灰的按钮记录
	private GameObject chooseRing;
	public static GameObject Dialog0;
	public static Transform trans;

	public BattleBlockData currentAttackBlock; //当前攻击的格子

	protected override void OnCreate()
	{
		InitTrans();
		
		//创建prefab 对所有子节点隐藏和显示控制的脚本
		childMgr = GameUtil.GetOrAddComponent<ChildGoComponent>(Skills_RectTransform);
		childMgr.Init(SkillItem_RectTransform);
		//currentSkill = 
		
		//初始化选择环
		chooseRing = Jyx2ResourceHelper.CreatePrefabInstance("CurrentBattleRoleTag");
		Transform pos = GameObject.FindWithTag("Player").transform;
		chooseRing.transform.position = pos.position;
		_buttonList = new Dictionary<Button, Action>();
		
		BindListener(Defend_Button, OnDefendClick);
		BindListener(Auto_Button, OnAutoClick);
		BindListener(Save_Button, OnSaveClick);
		BindListener(Catch_Button, OnCatchClick);
		
		BindListener(Strategy_Button, OnStrategyClick);
		BindListener(Item_Button, OnUseItemClick);
		
		BindListener(NormalAttack_Button, OnNormalAttackClick);
		
	}
	
	protected override void OnShowPanel(params object[] allParams)
	{
		base.OnShowPanel(allParams);
		m_currentRole = allParams[0] as RoleInstance;
		if (m_currentRole == null)
			return;
		callback = (Action<BattleManager.ManualResult>)allParams[1];
		
		//要切换其他人物的技能的话 在战斗前配置主角 
		var skills = m_currentRole.skills;
		if (skills.IsNullOrEmpty()) return;
		
		childMgr.RefreshChildCount(skills.Count); //按技能个数创建对象
		List<Transform> childTransList = childMgr.GetUsingTransList();//获得各技能位置
		skillList.Clear();
		//技能按钮绑定事件
		for (int i = 0; i < skills.Count; i++)
		{
			int index = i;
			//初始化所有技能 绑定事件
			SkillUIItem item = GameUtil.GetOrAddComponent<SkillUIItem>(childTransList[i]);
			item.RefreshSkill(skills[i]); 
			
			Button btn = item.GetComponent<Button>();
			BindListener(btn, () => { SkillButtonClick(item, index); }, false);
			skillList[skills[i]] = btn;
			
		}

		RefreshSkill();


		Dialog0 = transform.Find("Dialog").gameObject;
		trans = transform;
	}

	//获取玩家操作
	public  void FixedUpdate()
	{

		//base.Update();

		//寻找玩家点击的格子
		var block = InputManager.Instance.GetMouseUpBattleBlock();
		//射线没有找到格子
		if (block == null) return;
		var b = BattleManager.Instance.block_list.Find(b => b.blockName == block.name);
		if (b == null) return;
		
		//通过格子名称找到角色
		/*RoleInstance rol0 = null;
		BattleManager.Instance.Enermys.TryGetValue(b.blockName, out  rol0);
		if(rol0 == null) BattleManager.Instance.Teammates.TryGetValue(b.blockName, out rol0);*/
		//切换亮环位置
		currentAttackBlock = b;
		chooseRing.transform.position = b.WorldPos;
		//chooseRing.transform.GetComponent<MeshRenderer>().material.color = Color.red;
		
		Debug.Log("选择了格子:" + b.blockName);
		
		//移动
		//blockConfirm(block, true);
	}
	
	protected override void OnHidePanel()
	{
		base.OnHidePanel();
		skillList.Clear();
	}

	//技能按钮绑定的方法
	void SkillButtonClick(SkillUIItem item, int index)
	{
		Button btn = item.GetComponent<Button>();
		if(IsLocked(btn.GetComponentInChildren<Text>())) return;
		
		if (currentAttackBlock != null && item.GetSkill() != null)
		{
			callback?.Invoke(new BattleManager.ManualResult() { choose = "skillAttack",BlockData = currentAttackBlock,Skill = item.GetSkill()});
		}
		
		m_currentRole.SwitchAnimationToSkill(item.GetSkill());
	}

	void OnNormalAttackClick()
	{
		if(IsLocked(NormalAttack_Button.GetComponentInChildren<Text>())) return;
		if(currentAttackBlock != null) callback?.Invoke(new BattleManager.ManualResult() { choose = "normalAttack",BlockData = currentAttackBlock});
	}
	
	async void OnUseItemClick()
	{
		bool Filter(ItemInstance item) => (int)item.ItemType == 3 || (int)item.ItemType == 4;

		await UIManager.Instance.ShowUIAsync(nameof(BagUIPanel), GameRuntimeData.Instance.Player.Items, new Action<int>((itemId) =>
		{

			if (itemId == -1)
				return;

			var item = GameConfigDatabase.Instance.Get<ConfigItem>(itemId);
			if ((int)item.ItemType == 3) //使用道具逻辑
			{
				/*if (m_currentRole.CanUseItem(itemId))
				{
					//TryCallback(new BattleLoop.ManualResult() { aiResult = new AIResult() { Item = item } });
				}*/
			}

		}), (Func<ItemInstance, bool>)Filter);
	}
	
	
	//使用技能后 把Mp不足用的技能置为透明
	void RefreshSkill()
	{
		//先恢复置灰的按钮
		if (!greyButtons.IsNullOrEmpty())
		{
			foreach (var b in greyButtons)
			{
				b.transform.Find("Icon").GetComponent<Image>().color =new Color(1.0f,1.0f,1f,1f);
				b.transform.Find("SkillText").GetComponent<Text>().color =new Color(1.0f,1.0f,1f,1f) ;
			}
			greyButtons.Clear();
		}
		
		foreach (var skill in skillList.Keys)
		{
			if (skill.MpCost > m_currentRole.Mp)
			{
				skillList.TryGetValue(skill, out Button btn);
				btn.transform.Find("Icon").GetComponent<Image>().color =new Color(1.0f,1.0f,1f,0.3f);
				btn.transform.Find("SkillText").GetComponent<Text>().color =new Color(1.0f,1.0f,1f,0.3f) ;
				greyButtons.Add(btn);
			}

		}
	}

	//其余按钮 置灰和恢复方法
	void lockOtherButton(Button b,float f = -0.5f)
	{
		Color c0 = NormalAttack_Button.GetComponentInChildren<Text>().color;
		c0.a = c0.a + f;
		NormalAttack_Button.GetComponentInChildren<Text>().color = c0;
		
		Color c = new Color();Color c2 = new Color();
		foreach (var btn in RightActions_RectTransform.GetComponentsInChildren<Button>() )
		{
			if (!btn.Equals(b) && !btn.Equals(Item_Button)) //当前按钮和 策略组的子按钮不置透明
			{
				c = btn.GetComponentInChildren<Text>().color;
				c.a = c.a + f;
				btn.GetComponentInChildren<Text>().color = c; //文字也透明
				
				c2 = btn.image.color;
				c2.a = c2.a + f;
				btn.image.color = c2;
			}
			
		}
		foreach (var btn in LeftActions_RectTransform.GetComponentsInChildren<Button>() )
		{
			if (!btn.Equals(b) && !btn.Equals(Item_Button))
			{
				c = btn.GetComponentInChildren<Text>().color;
				c.a = c.a + f;
				btn.GetComponentInChildren<Text>().color = c; //文字也透明
				
				c2 = btn.image.color;
				c2.a = c2.a + f;
				btn.image.color = c2;
			}
		}
		foreach (var btn in Skills_RectTransform.GetComponentsInChildren<Button>() )
		{
			if (!btn.Equals(b) && !btn.Equals(Item_Button))
			{
				c = btn.transform.Find("Icon").GetComponent<Image>().color;
				c.a = c.a + f;
				btn.transform.Find("Icon").GetComponent<Image>().color = c; //文字也透明
				
				c2 = btn.transform.Find("SkillText").GetComponent<Text>().color;
				c2.a = c2.a + f;
				btn.transform.Find("SkillText").GetComponent<Text>().color= c2;
			}
		}
		

	}

	void OnDefendClick()
	{
		Text Text = Defend_Button.GetComponentInChildren<Text>();
		if (IsLocked(Text)) return; //按钮已被锁定不执行操作
		
		if (Text.text == "防御")
		{
			//文字内容改变，其余按钮透明度降低
			Text.text = "解除防御";
			lockOtherButton(Defend_Button);
		}
		else
		{
			Text.text = "防御";
			lockOtherButton(Defend_Button,0.5f);
		}
		//防御动作
		
	}
	void OnAutoClick()
	{
		Text Text = Auto_Button.GetComponentInChildren<Text>();
		if (IsLocked(Text)) return; //按钮已被锁定不执行操作
		
		if (Text.text == "自动")
		{
			//文字内容改变，其余按钮透明度降低
			Text.text = "解除自动";
			lockOtherButton(Auto_Button);
		}
		else
		{
			Text.text = "自动";
			lockOtherButton(Auto_Button,0.5f);
		}
		
	}
	
	void OnSaveClick()
	{
		Text Text = Save_Button.GetComponentInChildren<Text>();
		if (IsLocked(Text)) return; //按钮已被锁定不执行操作
		
		if (Text.text == "蓄力")
		{
			//文字内容改变，其余按钮透明度降低
			Text.text = "解除蓄力";
			lockOtherButton(Save_Button);
		}
		else
		{
			Text.text = "蓄力";
			lockOtherButton(Save_Button,0.5f);
		}
	}
	
	void OnCatchClick()
	{
		//TryCallback(new BattleLoop.ManualResult() { aiResult = new AIResult() { IsRest = true } });
	}
	
	void OnStrategyClick()
	{
		Text Text = Strategy_Button.GetComponentInChildren<Text>();
		if (IsLocked(Text)) return; //按钮已被锁定不执行操作
		
		if (Text.text == "进入策略")
		{
			//文字内容改变，其余按钮透明度降低
			Text.text = "结束策略";
			lockOtherButton(Strategy_Button);
			Item_Button.gameObject.SetActive(true); //物品按钮
			//暂停所有AI行动
			BattleManager.Instance.isPause = true;
		}
		else
		{
			Text.text = "进入策略";
			lockOtherButton(Strategy_Button,0.5f);
			Item_Button.gameObject.SetActive(false); //物品按钮
			BattleManager.Instance.isPause = false;
		}
		
		
	}

	public  Boolean IsLocked(Text te)
	{
		return te.color.a < 1.0f;
	}



}
