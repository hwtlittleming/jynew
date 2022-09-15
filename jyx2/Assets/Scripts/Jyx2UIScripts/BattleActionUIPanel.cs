
using Jyx2;
using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

public partial class BattleActionUIPanel : Jyx2_UIBase
{
	public override UILayer Layer => UILayer.NormalUI;
	public RoleInstance GetCurrentRole()
	{
		return m_currentRole;
	}

	RoleInstance m_currentRole;

	List<SkillUIItem> m_curItemList = new List<SkillUIItem>();
	ChildGoComponent childMgr;
	
	private Action<BattleManager.ManualResult> callback;
	private SkillInstance currentSkill;
	private Dictionary<Button, Action> skillList = new Dictionary<Button, Action>();
	private GameObject chooseRing;
	private GameObject chooseEnermyRing;
	public BattleBlockData currentAttackBlock; //当前攻击的格子

	protected override void OnCreate()
	{
		InitTrans();
		
		//初始化技能个数
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
		
		/*ShowUIAsync(nameof(BattleMainUIPanel), BattleMainUIState.None);
		BattleMainUIPanel.*/
	}
	
	protected override void OnShowPanel(params object[] allParams)
	{
		base.OnShowPanel(allParams);
		m_currentRole = allParams[0] as RoleInstance;
		if (m_currentRole == null)
			return;
		callback = (Action<BattleManager.ManualResult>)allParams[1];
		
		RefreshSkill();
		changeCurrentSelection(-1);
	}

	private int cur_zhaoshi = 0;
	

	protected override bool resetCurrentSelectionOnShow => false;

	//获取玩家操作
	public override void Update()
	{

		base.Update();

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
		chooseRing.transform.GetComponent<MeshRenderer>().material.color = Color.red;
		
		Debug.Log("选择了格子:" + b.blockName);
		
		//移动
		//blockConfirm(block, true);
	}

	protected override void buttonClickAt(int position)
	{
		if (!BattleboxHelper.Instance.AnalogMoved && cur_zhaoshi == -1)
			base.buttonClickAt(position);
	}
	
	protected override void OnHidePanel()
	{
		base.OnHidePanel();
		m_curItemList.Clear();
		
		skillList.Clear();
	}

	void RefreshSkill() //没蓝的技能要置灰 每次刷新重新加载技能未改是防以后要改为可操作其他人物 
	{
		m_curItemList.Clear();
		var zhaoshis = m_currentRole.skills.ToList();
		if (zhaoshis.IsNullOrEmpty()) return;
		childMgr.RefreshChildCount(zhaoshis.Count);
		List<Transform> childTransList = childMgr.GetUsingTransList();
		skillList.Clear();

		for (int i = 0; i < zhaoshis.Count; i++)
		{
			int index = i;
			//初始化所有技能 绑定事件
			SkillUIItem item = GameUtil.GetOrAddComponent<SkillUIItem>(childTransList[i]);
			item.RefreshSkill(zhaoshis[i]); 
			//当前选中的技能 框上 其余取消框； 第一次进时，当前技能为角色默认初始技能
			item.SetSelect(i == (currentSkill == null ?  m_currentRole.CurrentSkill : currentSkill.Key)); 

			Button btn = item.GetComponent<Button>();
			BindListener(btn, () => { SkillButtonClick(item, index); }, false);
			skillList[btn] = () => { SkillButtonClick(item, index); };

			m_curItemList.Add(item);
		}

		cur_zhaoshi = -1;
	}
	
	//技能按钮绑定的方法
	void SkillButtonClick(SkillUIItem item, int index)
	{
		//点击技能时去用技能攻击 第一次进不能直接在这里invoke 会直接setresult返回
		if (index != -1 && currentAttackBlock != null && item.GetSkill() != null)
		{
			callback?.Invoke(new BattleManager.ManualResult() { choose = "skillAttack",BlockData = currentAttackBlock,Skill = item.GetSkill()});
		}
		// clear current zhaoshi selection selected color only
		if (index > -1)
			changeCurrentSelection(-1);
		
		currentSkill = item.GetSkill();
		
		m_curItemList.ForEach(t =>
		{
			t.SetSelect(t == item);
		});
		
		m_currentRole.SwitchAnimationToSkill(item.GetSkill());
	}

	void OnNormalAttackClick()
	{
		if(currentAttackBlock != null) callback?.Invoke(new BattleManager.ManualResult() { choose = "normalAttack",BlockData = currentAttackBlock});
	}
	
	async void OnUseItemClick()
	{
		bool Filter(ItemInstance item) => (int)item.ItemType == 3 || (int)item.ItemType == 4;

		await Jyx2_UIManager.Instance.ShowUIAsync(nameof(BagUIPanel), GameRuntimeData.Instance.Player.Items, new Action<int>((itemId) =>
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

	void OnDefendClick()
	{
		Text Text = Defend_Button.GetComponentInChildren<Text>();
		if (IsLocked(Text)) return; //按钮已被锁定不执行操作
		
		if (Text.text == "防御")
		{
			//文字颜色置黄,内容改变，其余按钮透明度降低
			Color color = new Color(1f,0.5f,0.05f,1f); //按钮文字橙色
			Text.text = "解除防御";
			Text.color = color;
		
			lockOtherButton(Defend_Button);
		}
		else
		{
			Color color = new Color(1f,1f,0.6f,1f); //按钮原本颜色
			Text.text = "防御";
			Text.color = color;
			lockOtherButton(Defend_Button,0.5f);
		}

	}
	
	//使用技能后 把Mp不足用的技能置为透明
	void refreshSkill()
	{
		foreach (var skill in skillList.Keys)
		{
			/*if (skill.)
			{
				btn.transform.Find("Icon").GetComponent<Image>().color =new Color(1.0f,1.0f,1f,0.3f);
				btn.transform.Find("SkillText").GetComponent<Text>().color =new Color(1.0f,1.0f,1f,0.3f) ;
				
			}*/
		}
	}

	//其余按钮 置灰和恢复方法
	void lockOtherButton(Button b,float f = -0.5f)
	{
		NormalAttack_Button.gameObject.SetActive(false);
		
		Color c = new Color();Color c2 = new Color();
		foreach (var btn in RightActions_RectTransform.GetComponentsInChildren<Button>() )
		{
			if (!btn.Equals(b))
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
			if (!btn.Equals(b))
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
			if (!btn.Equals(b))
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

	void OnAutoClick()
	{
		Text Text = Auto_Button.GetComponentInChildren<Text>();
		if (IsLocked(Text)) return; //按钮已被锁定不执行操作
		
		if (Text.text == "自动")
		{
			//文字颜色置黄,内容改变，其余按钮透明度降低
			Color color = new Color(1f,0.5f,0.05f,1f); //按钮文字橙色
			Text.text = "解除自动";
			Text.color = color;
		
			lockOtherButton(Auto_Button);
		}
		else
		{
			Color color = new Color(1f,1f,0.6f,1f); //按钮原本颜色
			Text.text = "自动";
			Text.color = color;
			lockOtherButton(Auto_Button,0.5f);
		}
	}
	
	void OnSaveClick()
	{
		//TryCallback(new BattleLoop.ManualResult() { aiResult = new AIResult() { IsRest = true } });
	}
	
	void OnCatchClick()
	{
		//TryCallback(new BattleLoop.ManualResult() { aiResult = new AIResult() { IsRest = true } });
	}
	
	void OnStrategyClick()
	{
		//TryCallback(new BattleLoop.ManualResult() { aiResult = new AIResult() { IsRest = true } });
	}
	public void OnAutoClicked()
	{
		//TryCallback(new BattleLoop.ManualResult() { isAuto = true });
	}
	
	public  Boolean IsLocked(Text te)
	{
		return te.color.a >= 255;
	}



}
