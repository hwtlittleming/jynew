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
		RefreshEquipments(m_currentRole);

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

		sb.AppendLine(string.Format("生命 <color={0}>{1}</color>/<color={2}>{3}</color>".GetContent(nameof(XiakeUIPanel)), color1, role.Hp, color2,
			role.MaxHp));
		sb.Append(string.Format("能量 <color={0}>{1}/{2}</color>".GetContent(nameof(XiakeUIPanel)), color, role.Mp, role.MaxMp));
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
		//---------------------------------------------------------------------------
		//---------------------------------------------------------------------------
		
		return sb.ToString();
	}

	//判断 第二列离第一列要生成多少个transparent透明数字位(为了保持列距整齐)
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
			sb.AppendLine(skill.Name + " " + skill.GetLevel());
		}
		sb.AppendLine("<color=#FF9610>--绝技" + "--</color>");
		return sb.ToString();
	}

	void RefreshEquipments(RoleInstance role)
	{
		for (int i = 0; i< Equipments.childCount; i++)
		{
			Transform transform = Equipments.GetChild(i);
			Text m_roleName = transform.Find("NameText").GetComponent<Text>();
			Image m_roleHead = transform.Find("Image").GetComponent<Image>();
			if (transform.name.Contains("Weapon"))
			{
				Jyx2ConfigItem item = role.Equipments[0];
				if (item !=null)
				{
					m_roleName.text = item.Name;
					m_roleHead.LoadAsyncForget(item.GetPic());
				}else
				{
					m_roleHead.gameObject.SetActive(false);
					m_roleName.text = "武器";
				}
			}
			else if (transform.name.Contains("Armor"))
			{
				Jyx2ConfigItem item = role.Equipments[1];
				if (item !=null)
				{
					m_roleName.text = item.Name;
					m_roleHead.LoadAsyncForget(item.GetPic());
				}else
				{
					m_roleHead.gameObject.SetActive(false);
					m_roleName.text = "防具";
				}
			}
			else if (transform.name.Contains("Shoes"))
			{
				Jyx2ConfigItem item = role.Equipments[2];
				if (item !=null)
				{
					m_roleName.text = item.Name;
					m_roleHead.LoadAsyncForget(item.GetPic());
				}else
				{
					m_roleHead.gameObject.SetActive(false);
					m_roleName.text = "代步";
				}
			}
			else if (transform.name.Contains("Treasure"))
			{
				Jyx2ConfigItem item = role.Equipments[3];
				if (item !=null)
				{
					m_roleName.text = item.Name;
					m_roleHead.LoadAsyncForget(item.GetPic());
				}else
				{
					m_roleHead.gameObject.SetActive(false);
					m_roleName.text = "宝物";
				}
			}
		}
		
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

	async void OnEquipmentClick(int index)
	{
		Jyx2ConfigItem.Jyx2ConfigItemEquipmentType type = Jyx2ConfigItem.Jyx2ConfigItemEquipmentType.武器;
		Jyx2ConfigItem yetItem = m_currentRole.Equipments[index] != null ? m_currentRole.Equipments[index] : new Jyx2ConfigItem();
		switch (index)
		{
			case 0:
				type = Jyx2ConfigItem.Jyx2ConfigItemEquipmentType.武器;
				break;
			case 1:
				type = Jyx2ConfigItem.Jyx2ConfigItemEquipmentType.防具;
				break;
			case 2:
				type = Jyx2ConfigItem.Jyx2ConfigItemEquipmentType.代步;
				break;
			case 3:
				type = Jyx2ConfigItem.Jyx2ConfigItemEquipmentType.宝物;
				break;
		}
		await SelectFromBag(
			(itemId) =>
			{
				var item = GameConfigDatabase.Instance.Get<Jyx2ConfigItem>(itemId);
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
					m_currentRole.Equipments[index] = GameConfigDatabase.Instance.Get<Jyx2ConfigItem>(itemId);//替换存储的角色装备
					m_currentRole.UseItem(m_currentRole.Equipments[index]); //加减属性
					runtime.SetItemUser(item.Id, m_currentRole.GetJyx2RoleId());
				}
			},
			(item) => { return item.EquipmentType ==type && (runtime.GetItemUser(item.Id) == m_currentRole.GetJyx2RoleId() || runtime.GetItemUser(item.Id) == -1); },
			m_currentRole.Equipments[index] == null ? -1 : m_currentRole.Equipments[index].Id);
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