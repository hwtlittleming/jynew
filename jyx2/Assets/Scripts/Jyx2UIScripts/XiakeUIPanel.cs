

using Jyx2;
using Jyx2.Middleware;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;
using UnityEngine;
using UnityEngine.UI;

using Jyx2Configs;
using Vector3 = UnityEngine.Vector3;

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
		
		BindListener(ButtonSelectWeapon_Button, () => OnEquipmentClick(0));
		BindListener(ButtonSelectArmor_Button, () => OnEquipmentClick(1));
		BindListener(ButtonSelectShoes_Button, () => OnEquipmentClick(2));
		BindListener(ButtonSelectTreasure_Button, () => OnEquipmentClick(3));
		BindListener(LeaveButton_Button, OnLeaveClick);
	}


	protected override void OnShowPanel(params object[] allParams)
	{
		base.OnShowPanel(allParams);
		m_currentRole = allParams[0] as RoleInstance;
		if (allParams.Length > 1)
			m_roleList = allParams[1] as List<RoleInstance>;
		
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
		PreImage_Image.LoadAsyncForget(m_currentRole.configData.GetPic());
		
		InfoText_Text.text = GetInfoText(m_currentRole);
		SkillText_Text.text = GetSkillText(m_currentRole);
		RefreshEquipments(m_currentRole);

		//select the first available button
		changeCurrentSelection(0);
	}

	//左侧人物选择下拉框
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

		sb.AppendLine(string.Format("生命 <color={0}>{1}</color>/<color={2}>{3}</color>".GetContent(nameof(XiakeUIPanel)), color1, role.Hp, color2,
			role.MaxHp));
		sb.Append(string.Format(("能量 <color={0}>{1}/{2}</color>" + FlexibleLength(role.Strength)).GetContent(nameof(XiakeUIPanel)), color, role.Mp, role.MaxMp));
		sb.AppendLine(string.Format("状态 {0}        ".GetContent(nameof(XiakeUIPanel)), role.State));
		sb.AppendLine(string.Format("战斗经验 {0}/{1}".GetContent(nameof(XiakeUIPanel)), role.Exp, role.GetLevelUpExp()));
		
		sb.AppendLine("<color=#FF9610>--资质" + "--</color>");
		sb.Append(string.Format(("力量 {0}" + FlexibleLength(role.Strength)).GetContent(nameof(XiakeUIPanel)), role.Strength));
		sb.AppendLine(string.Format("智慧 {0}".GetContent(nameof(XiakeUIPanel)), role.IQ));
		sb.Append(string.Format(("体质 {0}" + FlexibleLength(role.Constitution)).GetContent(nameof(XiakeUIPanel)), role.Constitution));
		sb.AppendLine(string.Format("敏捷 {0}".GetContent(nameof(XiakeUIPanel)), role.Agile));
		sb.AppendLine("<color=#FF9610>--属性" + "--</color>");
		sb.Append(string.Format(("攻击 {0}" + FlexibleLength(role.Attack)).GetContent(nameof(XiakeUIPanel)), role.Attack));
		sb.AppendLine(string.Format("防御 {0}".GetContent(nameof(XiakeUIPanel)), role.Defense));
		sb.Append(string.Format(("速度 {0}" + FlexibleLength(role.Speed)).GetContent(nameof(XiakeUIPanel)), role.Speed));
		sb.AppendLine(string.Format("回复 {0}".GetContent(nameof(XiakeUIPanel)), role.Heal));
		sb.Append(string.Format(("暴击 {0}" + FlexibleLength(role.Critical)).GetContent(nameof(XiakeUIPanel)), role.Critical,role.CriticalLevel));
		sb.AppendLine(string.Format("闪避 {0}".GetContent(nameof(XiakeUIPanel)), role.Miss));
		sb.AppendLine(string.Format(("幸运 {0}" + FlexibleLength(role.Luck)).GetContent(nameof(XiakeUIPanel)), role.Luck));
		sb.AppendLine(string.Format("描述: {0}".GetContent(nameof(XiakeUIPanel)), role.Describe));
        		
		sb.AppendLine();
		return sb.ToString();
	}

	//判断 第二列离第一列要生成多少个transparent透明数字位(为了保持列距整齐) 未计算中文
	StringBuilder FlexibleLength(int attr,int maxLength = 9)
	{
		int count = 0;
		StringBuilder result = new StringBuilder();
		if (attr < 0)
		{
			attr = Math.Abs(attr);
			count++;
		}else if (attr == 0)
		{
			return new StringBuilder("            ");//12个
		}
		while (attr > 0)
		{
			attr /= 10;
			count++;
		}
		count = Math.Abs(maxLength - count);
		result.Append("<color=transparent>");
		while (count > 0)
		{
			result.Append("0");
			count--;
		}
		result.Append("</color>");
		return result;
	}

	string GetSkillText(RoleInstance role)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("<color=#FF9610>--奇术" + "--</color>");
		foreach (var skill in role.skills)
		{
			sb.AppendLine(skill.Name + " " + skill.Level);
		}
		sb.AppendLine("<color=#FF9610>--绝技" + "--</color>");
		return sb.ToString();
	}

	void RefreshEquipments(RoleInstance role)
	{
		foreach (var equip in role.Equipments)
		{
			if(equip == null) continue;
			Text equipName = ButtonSelectWeapon_Button.transform.Find("NameText").GetComponent<Text>();
			Image equipHead = ButtonSelectWeapon_Button.transform.Find("Image").GetComponent<Image>();
			
			int type = (int) equip.ItemType;
			if (type == 10)
			{
			}else if (type == 11)
			{
				equipName = ButtonSelectArmor_Button.transform.Find("NameText").GetComponent<Text>();
				equipHead = ButtonSelectArmor_Button.transform.Find("Image").GetComponent<Image>();
			}else if (type == 12)
			{
				equipName = ButtonSelectShoes_Button.transform.Find("NameText").GetComponent<Text>();
				equipHead = ButtonSelectShoes_Button.transform.Find("Image").GetComponent<Image>();
			}else if (type == 13)
			{
				equipName = ButtonSelectTreasure_Button.transform.Find("NameText").GetComponent<Text>();
				equipHead = ButtonSelectTreasure_Button.transform.Find("Image").GetComponent<Image>();
			}
			equipName.text = equip.Name;
			equipHead.LoadAsyncForget(equip.GetPic());
		}
	}

	void OnBackClick()
	{
		Jyx2_UIManager.Instance.HideUI(nameof(XiakeUIPanel));
	}

	//人物离队
	void OnLeaveClick()
	{
		if (m_currentRole == null)
			return;
		if (!m_roleList.Contains(m_currentRole))
			return;
		if (m_currentRole.Id == GameRuntimeData.Instance.Player.Id)
		{
			GameUtil.DisplayPopinfo("主角不能离开队伍");
			return;
		}
		var eventLuaPath = GameConfigDatabase.Instance.Get<Jyx2ConfigCharacter>(m_currentRole.Id).LeaveStoryId;
		if (!string.IsNullOrEmpty(eventLuaPath))
		{
			Jyx2.LuaExecutor.Execute("jygame/ka" + eventLuaPath, RefreshView);
		}
		else
		{
			GameRuntimeData.Instance.LeaveTeam(m_currentRole.Id);
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

	async void OnEquipmentClick(int index)
	{
		ItemInstance yetItem =  m_currentRole.Equipments[index];
		await SelectFromBag(
			(itemId) =>
			{
				var item = runtime.AllRoles[0].GetItem(itemId);
				//选择了当前使用的装备，则卸下
				if ( yetItem!=null && yetItem.Id == itemId)
				{
					m_currentRole.UnequipItem(yetItem,index);
					//反射有方法给特定对象的属性赋值
					if (m_currentRole.Equipments[index] !=null)
					{
						m_currentRole.Equipments[index] = null;
					}
				}
				//否则更新
				else
				{
					m_currentRole.UnequipItem(yetItem,index); //卸下原装备
					m_currentRole.Equipments[index] = item;//替换存储的角色装备
					m_currentRole.UseItem(m_currentRole.Equipments[index]); //加减属性
					runtime.SetItemUser(item.Id, m_currentRole.Id);
				}
			},
			(item) => { return (int)item.ItemType == (index + 10) && (runtime.GetItemUser(item) == m_currentRole.Id || runtime.GetItemUser(item) == -1); },
			m_currentRole.Equipments[index] == null ? null : m_currentRole.Equipments[index].Id );
	}
	
	async UniTask SelectFromBag(Action<String> callback, Func<ItemInstance, bool> filter, String currentItemId)
	{
		await Jyx2_UIManager.Instance.ShowUIAsync(nameof(BagUIPanel), runtime.AllRoles[0].Items, new Action<String>((itemId) =>
		{
			if (itemId != null && !m_currentRole.CanUseItem(itemId))
			{
				var item = GameRuntimeData.Instance.AllRoles[0].GetItem(itemId);
				GameUtil.DisplayPopinfo((int)item.ItemType == 1 ? "此人不适合配备此物品" : "此人不适合修炼此物品");
				return;
			}

			if (itemId != null)
			{
				//卸下或使用选择装备
				callback(itemId);
			}

			RefreshCurrent();
		}), filter, currentItemId);
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