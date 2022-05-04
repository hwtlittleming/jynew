/*
 * 金庸群侠传3D重制版
 * https://github.com/jynew/jynew
 *
 * 这是本开源项目文件头，所有代码均使用MIT协议。
 * 但游戏内资源和第三方插件、dll等请仔细阅读LICENSE相关授权协议文档。
 *
 * 金庸老先生千古！
 */

using Jyx2;
using Jyx2.Middleware;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;
using UnityEngine;
using UnityEngine.UI;

using Jyx2Configs;

public partial class XiakeUIPanel : Jyx2_UIBase
{
	public override UILayer Layer => UILayer.NormalUI;

	RoleInstance m_currentRole;
	List<RoleInstance> m_roleList;
	RoleUIItem m_currentShowItem;
	private int m_currentRole_index = 0;
	private List<RoleUIItem> m_roleUIItems = new List<RoleUIItem>();

	protected override void OnCreate()
	{
		InitTrans();
		IsBlockControl = true;

		//there is button for this, so doesn't get into the listing of dpad nav
		BindListener(BackButton_Button, OnBackClick, false);
		
		BindListener(ButtonSelectWeapon_Button, OnWeaponClick);
		BindListener(ButtonSelectArmor_Button, OnArmorClick);
		BindListener(LeaveButton_Button, OnLeaveClick);
	}


	protected override void OnShowPanel(params object[] allParams)
	{
		base.OnShowPanel(allParams);
		m_currentRole = allParams[0] as RoleInstance;
		if (allParams.Length > 1)
			m_roleList = allParams[1] as List<RoleInstance>;

		/*var curMap=GameRuntimeData.Instance.CurrentMap;
        (LeaveButton_Button.gameObject).SetActive("0_BigMap"==curMap);*/
		DoRefresh();
	}

	void DoRefresh()
	{
		RefreshScrollView();
		RefreshCurrent();
	}

	private void OnEnable()
	{
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.Escape, OnBackClick);
	}

	private void OnDisable()
	{
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.Escape);
	}

	protected override void OnHidePanel()
	{
		m_currentRole_index = 0;
		base.OnHidePanel();
		HSUnityTools.DestroyChildren(RoleParent_RectTransform);
	}

	void RefreshCurrent()
	{
		if (m_currentRole == null)
		{
			Debug.LogError("has not current role");
			return;
		}

		NameText_Text.text = m_currentRole.Name;

		InfoText_Text.text = GetInfoText(m_currentRole);
		SkillText_Text.text = GetSkillText(m_currentRole);
		ItemsText_Text.text = GetItemsText(m_currentRole);

		//select the first available button
		changeCurrentSelection(0);

		PreImage_Image.LoadAsyncForget(m_currentRole.Data.GetPic());
	}

	void RefreshScrollView()
	{
		m_roleUIItems.Clear();
		HSUnityTools.DestroyChildren(RoleParent_RectTransform);
		if (m_roleList == null || m_roleList.Count <= 0)
			return;
		RoleInstance role;
		cleanupDestroyedButtons();
		for (int i = 0; i < m_roleList.Count; i++)
		{
			role = m_roleList[i];
			var item = RoleUIItem.Create();
			m_roleUIItems.Add(item);
			item.transform.SetParent(RoleParent_RectTransform);
			item.transform.localScale = Vector3.one;

			Button btn = item.GetComponent<Button>();
			BindListener(btn, () => { OnItemClick(item); }, false);
			bool isSelect = (m_currentRole == role);
			if (isSelect)
			{
				m_currentShowItem = item;
				m_currentRole_index = i;
			}
			item.SetState(isSelect, false);
			item.ShowRole(role);
		}
	}

	void OnItemClick(RoleUIItem item)
	{
		if (m_currentShowItem != null && m_currentShowItem == item)
			return;

		if (m_currentShowItem)
			m_currentShowItem.SetState(false, false);

		m_currentShowItem = item;
		m_currentShowItem.SetState(true, false);

		m_currentRole = m_currentShowItem.GetShowRole();
		RefreshCurrent();
	}

	string GetInfoText(RoleInstance role)
	{
		StringBuilder sb = new StringBuilder();
		var color = role.GetMPColor();
		var color1 = role.GetHPColor1();
		var color2 = role.GetHPColor2();

		//---------------------------------------------------------------------------
		//特定位置的翻译【XiakePanel角色信息显示大框的信息】
		//---------------------------------------------------------------------------
		sb.AppendLine(string.Format("等级 {0}".GetContent(nameof(XiakeUIPanel)), role.Level));
		sb.AppendLine(string.Format("经验 {0}/{1}".GetContent(nameof(XiakeUIPanel)), role.Exp, role.GetLevelUpExp()));
		sb.AppendLine(string.Format("生命 <color={0}>{1}</color>/<color={2}>{3}</color>".GetContent(nameof(XiakeUIPanel)), color1, role.Hp, color2,
			role.MaxHp));
		sb.AppendLine(string.Format("能量 <color={0}>{1}/{2}</color>".GetContent(nameof(XiakeUIPanel)), color, role.Mp, role.MaxMp));

		sb.AppendLine();
		sb.AppendLine(string.Format("力量 {0}".GetContent(nameof(XiakeUIPanel)), role.Strength));
		sb.AppendLine(string.Format("智慧 {0}".GetContent(nameof(XiakeUIPanel)), role.IQ));
		sb.AppendLine(string.Format("根骨 {0}".GetContent(nameof(XiakeUIPanel)), role.Constitution));
		sb.AppendLine(string.Format("敏捷 {0}".GetContent(nameof(XiakeUIPanel)), role.Agile));
		sb.AppendLine(string.Format("幸运 {0}".GetContent(nameof(XiakeUIPanel)), role.Luck));
		sb.AppendLine();
		sb.AppendLine(string.Format("攻击 {0}".GetContent(nameof(XiakeUIPanel)), role.Attack));
		sb.AppendLine(string.Format("防御 {0}".GetContent(nameof(XiakeUIPanel)), role.Defense));
		sb.AppendLine(string.Format("速度 {0}".GetContent(nameof(XiakeUIPanel)), role.Speed));
		sb.AppendLine(string.Format("回复 {0}".GetContent(nameof(XiakeUIPanel)), role.Heal));
		sb.AppendLine(string.Format("暴击 {0}%--{1}倍".GetContent(nameof(XiakeUIPanel)), role.Critical,role.CriticalLevel));
		sb.AppendLine(string.Format("闪避 {0}%".GetContent(nameof(XiakeUIPanel)), role.Miss));
		sb.AppendLine();
		//---------------------------------------------------------------------------
		//---------------------------------------------------------------------------
		
		return sb.ToString();
	}

	string GetSkillText(RoleInstance role)
	{
		StringBuilder sb = new StringBuilder();
		foreach (var skill in role.skills)
		{
			sb.AppendLine(skill.Name + " " + skill.GetLevel());
		}

		return sb.ToString();
	}

	string GetItemsText(RoleInstance role)
	{
		StringBuilder sb = new StringBuilder();
		var weapon = role.GetWeapon();
		//特定位置的翻译【XiakePanel角色信息显示大框的信息】
		//---------------------------------------------------------------------------
		sb.AppendLine("武器：".GetContent(nameof(XiakeUIPanel)) + (weapon == null ? "" : weapon.Name));

		var armor = role.GetArmor();
		sb.AppendLine("防具：".GetContent(nameof(XiakeUIPanel)) + (armor == null ? "" : armor.Name));
		
		var shoes = role.GetShoes();
		sb.AppendLine("代步：".GetContent(nameof(XiakeUIPanel)) + (shoes == null ? "" : shoes.Name));
		
		var treasure = role.GetTreasure();
		sb.AppendLine("宝物：".GetContent(nameof(XiakeUIPanel)) + (treasure == null ? "" : treasure.Name));
		//---------------------------------------------------------------------------
		//---------------------------------------------------------------------------

		return sb.ToString();
	}

	void OnBackClick()
	{
		Jyx2_UIManager.Instance.HideUI(nameof(XiakeUIPanel));
	}

	// added handle leave chat logic
	// by eaphone at 2021/6/6
	void OnLeaveClick()
	{

		var curMap = LevelMaster.GetCurrentGameMap();

		if (m_currentRole == null)
			return;
		if (!m_roleList.Contains(m_currentRole))
			return;
		if (m_currentRole.GetJyx2RoleId() == GameRuntimeData.Instance.Player.GetJyx2RoleId())
		{
			GameUtil.DisplayPopinfo("主角不能离开队伍");
			return;
		}

		var eventLuaPath = GameConfigDatabase.Instance.Get<Jyx2ConfigCharacter>(m_currentRole.GetJyx2RoleId()).LeaveStoryId;
		if (!string.IsNullOrEmpty(eventLuaPath))
		{
			Jyx2.LuaExecutor.Execute("jygame/ka" + eventLuaPath, RefreshView);
		}
		else
		{
			GameRuntimeData.Instance.LeaveTeam(m_currentRole.GetJyx2RoleId());
			RefreshView();
		}
	}

	void RefreshView()
	{
		m_roleList.Remove(m_currentRole);
		m_currentRole = GameRuntimeData.Instance.Player;
		DoRefresh();
	}

	GameRuntimeData runtime
	{
		get { return GameRuntimeData.Instance; }
	}

	async void OnWeaponClick()
	{
		await SelectFromBag(
			(itemId) =>
			{
				var item = GameConfigDatabase.Instance.Get<Jyx2ConfigItem>(itemId);

				//选择了当前使用的装备，则卸下
				if (m_currentRole.Weapon == itemId)
				{
					m_currentRole.UnequipItem(m_currentRole.GetWeapon());
					m_currentRole.Weapon = -1;
				}
				//否则更新
				else
				{
					m_currentRole.UnequipItem(m_currentRole.GetWeapon());
					m_currentRole.Weapon = itemId;
					m_currentRole.UseItem(m_currentRole.GetWeapon());
					runtime.SetItemUser(item.Id, m_currentRole.GetJyx2RoleId());
				}
			},
			(item) => { return item.EquipmentType == 0 && (runtime.GetItemUser(item.Id) == m_currentRole.GetJyx2RoleId() || runtime.GetItemUser(item.Id) == -1); },
			m_currentRole.Weapon);
	}

	async void OnArmorClick()
	{
		await SelectFromBag(
			(itemId) =>
			{
				var item = GameConfigDatabase.Instance.Get<Jyx2ConfigItem>(itemId);
				if (m_currentRole.Armor == itemId)
				{
					m_currentRole.UnequipItem(m_currentRole.GetArmor());
					m_currentRole.Armor = -1;
				}
				else
				{
					m_currentRole.UnequipItem(m_currentRole.GetArmor());
					m_currentRole.Armor = itemId;
					m_currentRole.UseItem(m_currentRole.GetArmor());
					runtime.SetItemUser(item.Id, m_currentRole.GetJyx2RoleId());
				}
			},
			(item) => { return (int)item.EquipmentType == 1 && (runtime.GetItemUser(item.Id) == m_currentRole.GetJyx2RoleId() || runtime.GetItemUser(item.Id) == -1); },
			m_currentRole.Armor);
	}
	

	async UniTask SelectFromBag(Action<int> Callback, Func<Jyx2ConfigItem, bool> filter, int current_itemId)
	{
		await Jyx2_UIManager.Instance.ShowUIAsync(nameof(BagUIPanel), runtime.Items, new Action<int>((itemId) =>
		{
			if (itemId != -1 && !m_currentRole.CanUseItem(itemId))
			{
				var item = GameConfigDatabase.Instance.Get<Jyx2ConfigItem>(itemId);
				GameUtil.DisplayPopinfo((int)item.ItemType == 1 ? "此人不适合配备此物品" : "此人不适合修炼此物品");
				return;
			}

			if (itemId != -1)
			{
				//卸下或使用选择装备
				Callback(itemId);
			}

			RefreshCurrent();
		}), filter, current_itemId);
	}

	protected override bool captureGamepadAxis => true;

	protected override void handleGamepadButtons()
	{
		base.handleGamepadButtons();
		if (gameObject.activeSelf)
		{
			if (GamepadHelper.IsCancel())
			{
				OnBackClick();
			}
			else if (GamepadHelper.IsTabLeft())
			{
				selectPreviousRole();
			}
			else if (GamepadHelper.IsTabRight())
			{
				selectNextRole();
			}
		}
	}

	private void selectPreviousRole()
	{
		if (m_currentRole_index == 0)
		{
			m_currentRole_index = m_roleUIItems.Count - 1;
		}
		else
			m_currentRole_index--;

		OnItemClick(m_roleUIItems[m_currentRole_index]);
	}

	private void selectNextRole()
	{
		if (m_currentRole_index == m_roleUIItems.Count - 1)
		{
			m_currentRole_index = 0;
		}
		else
			m_currentRole_index++;

		OnItemClick(m_roleUIItems[m_currentRole_index]);
	}
}