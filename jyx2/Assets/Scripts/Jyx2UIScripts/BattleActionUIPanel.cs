
using Jyx2;
using Jyx2.Middleware;

using Jyx2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Cysharp.Threading.Tasks;
using Jyx2.Battle;
using Jyx2Configs;
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
	private BattleZhaoshiInstance currentZhaoshi;
	private Dictionary<Button, Action> zhaoshiList = new Dictionary<Button, Action>();
	private GameObject chooseRing;
	private GameObject chooseEnermyRing;
	public BattleBlockData currentAttackBlock;

	protected override void OnCreate()
	{
		InitTrans();
		childMgr = GameUtil.GetOrAddComponent<ChildGoComponent>(Skills_RectTransform);
		childMgr.Init(SkillItem_RectTransform);
		
		chooseRing = Jyx2ResourceHelper.CreatePrefabInstance("CurrentBattleRoleTag");
		Transform pos = GameObject.FindWithTag("Player").transform;
		chooseRing.transform.position = pos.position;
		_buttonList = new Dictionary<Button, Action>();
		
		BindListener(Move_Button, OnMoveClick);
		BindListener(Item_Button, OnUseItemClick);
		BindListener(Rest_Button, OnRestClick);
		BindListener(NormalAttack_Button, OnNormalAttackClick);
		
		/*ShowUIAsync(nameof(BattleMainUIPanel), BattleMainUIState.None);
		BattleMainUIPanel.*/
	}

	protected override bool captureGamepadAxis { get { return true; } }

	protected override Text getButtonText(Button button)
	{
		if (button.gameObject.transform.childCount == 1)
			return base.getButtonText(button);

		for (var i = 0; i < button.gameObject.transform.childCount; i++)
		{
			var text = button.gameObject.transform.GetChild(i).GetComponent<Text>();
			if (text != null)
				return text;
		}

		return null;
	}

	protected Image getZhaoshiButtonImage(Button button)
	{
		Transform trans = button.gameObject.transform;
		for (var i = 0; i < trans.childCount; i++)
		{
			var image = trans.GetChild(i).GetComponent<Image>();
			if (image != null && image.name == "ActionIcon")
				return image;
		}

		return null;
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

	private void changeCurrentZhaoshiSelection(int number)
	{
		if (zhaoshiList.Count == 0)
			return;

		cur_zhaoshi = number;

		if (number > -1)
		{
			changeCurrentSelection(-1);
			BattleboxHelper.Instance.AnalogMoved = false;
		}

		var curBtnKey = number < 0 || number > zhaoshiList.Count ?
			null :
			zhaoshiList.ElementAt(number).Key;

		foreach (var btn in zhaoshiList)
		{
			bool isInvokedButton = btn.Key == curBtnKey;
			var text = getButtonText(btn.Key);
			if (text != null)
			{
				text.color = isInvokedButton ?
					base.selectedButtonColor() :
					base.normalButtonColor();
				text.fontStyle = isInvokedButton ?
					FontStyle.Bold :
					FontStyle.Normal;
			}

			var action = getZhaoshiButtonImage(btn.Key);
			if (action != null)
			{
				action.gameObject.SetActive(isInvokedButton);
			}
		}
	}

	protected override void changeCurrentSelection(int num)
	{
		if (num > -1)
		{
			changeCurrentZhaoshiSelection(-1);
			BattleboxHelper.Instance.AnalogMoved = false;
		}

		base.changeCurrentSelection(num);
	}

	protected override bool resetCurrentSelectionOnShow => false;

	public override void Update()
	{

		base.Update();

		//寻找玩家点击的格子
		var block = InputManager.Instance.GetMouseUpBattleBlock();
		//射线没有找到格子
		if (block == null) return;
		var b = BattleManager.Instance.block_list.Find(b => b.blockName == block.name);
		if (b == null) return;

		if (b.role!=null)//格子上有人就切换亮环位置
		{
			if (b.blockName.StartsWith("we"))
			{
				chooseRing.transform.position = b.WorldPos;
			}
			else
			{
				currentAttackBlock = b;
				if (chooseEnermyRing == null)
				{
					chooseEnermyRing = Jyx2ResourceHelper.CreatePrefabInstance("CurrentBattleRoleTag");
					chooseEnermyRing.transform.GetComponent<MeshRenderer>().material.color = Color.red;
				}
				chooseEnermyRing.transform.position = b.WorldPos;
			}
		}
		Debug.Log("选择了格子:" + b.blockName);
		
		//移动
		//blockConfirm(block, true);
	}

	protected override void buttonClickAt(int position)
	{
		if (!BattleboxHelper.Instance.AnalogMoved && cur_zhaoshi == -1)
			base.buttonClickAt(position);
	}

	//点击了自动
	public void OnAutoClicked()
	{
		//TryCallback(new BattleLoop.ManualResult() { isAuto = true });
	}

	protected override void OnHidePanel()
	{
		base.OnHidePanel();
		m_currentRole = null;
		m_curItemList.Clear();

		//隐藏格子
		BattleboxHelper.Instance?.HideAllBlocks();
		zhaoshiList.Clear();
	}

	void RefreshSkill()
	{
		m_curItemList.Clear();
		var zhaoshis = m_currentRole.GetZhaoshis(true).ToList();
		if (zhaoshis.IsNullOrEmpty()) return;
		childMgr.RefreshChildCount(zhaoshis.Count);
		List<Transform> childTransList = childMgr.GetUsingTransList();
		zhaoshiList.Clear();

		for (int i = 0; i < zhaoshis.Count; i++)
		{
			int index = i;
			SkillUIItem item = GameUtil.GetOrAddComponent<SkillUIItem>(childTransList[i]);
			item.RefreshSkill(zhaoshis[i]);
			item.SetSelect(i == m_currentRole.CurrentSkill);

			Button btn = item.GetComponent<Button>();
			bindZhaoshi(btn, () => { onZhaoshiStart(item, index); });
			m_curItemList.Add(item);
		}

		changeCurrentZhaoshiSelection(-1);
	}

	void bindZhaoshi(Button btn, Action callback)
	{
		BindListener(btn, callback, false);
		zhaoshiList[btn] = callback;
	}
	
	//上面bindZhaoshi时 将每个技能按钮和该方法绑定了
	void onZhaoshiStart(SkillUIItem item, int index)
	{
		//点击技能时去用技能攻击 第一次进不能直接在这里invoke 会直接setresult返回
		if (index != -1 && currentAttackBlock != null && item.GetSkill() != null)
		{
			callback?.Invoke(new BattleManager.ManualResult() { choose = "skillAttack",BlockData = currentAttackBlock,Skill = item.GetSkill()});
		}
		// clear current zhaoshi selection selected color only
		if (index > -1)
			changeCurrentSelection(-1);
		
		currentZhaoshi = item.GetSkill();
		
		m_curItemList.ForEach(t =>
		{
			t.SetSelect(t == item);
		});
		
		m_currentRole.SwitchAnimationToSkill(item.GetSkill().Data);
	}

	void OnNormalAttackClick()
	{
		if(currentAttackBlock != null) callback?.Invoke(new BattleManager.ManualResult() { choose = "normalAttack",BlockData = currentAttackBlock});
	}

	async void OnMoveClick()
	{
		
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
	void OnRestClick()
	{
		//TryCallback(new BattleLoop.ManualResult() { aiResult = new AIResult() { IsRest = true } });
	}
}
