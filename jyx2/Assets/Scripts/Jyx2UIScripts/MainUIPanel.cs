
using Jyx2;
using UnityEngine;
using System;
using System.Linq;
using Jyx2Configs;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;
using UnityEngine.UI;

public partial class MainUIPanel : Jyx2_UIBase, IUIAnimator
{
	public override UILayer Layer => UILayer.MainUI;

	protected override void OnCreate()
	{
		InitTrans();

		BindListener(XiakeButton_Button, OnXiakeBtnClick);
		BindListener(BagButton_Button, OnBagBtnClick);
		BindListener(SystemButton_Button, OnSystemBtnClick);
		
		BindListener(Save_Button, OnSaveBtnClick);
		BindListener(Load_Button, OnLoadBtnClick);
		BindListener(MainMenu_Button, OnMainMenuBtnClick);
		BindListener(Settings_Button, OnSettingsBtnClick);
		BindListener(Close_Button, OnCloseBtnClick);
		
		//pre-load all icon sprites. somehow they don't load the first time
		foreach (var i in Enumerable.Range(0, 4))
			ChatUIPanel.getGamepadIconSprites(i);
	}

	public override void BindListener(UnityEngine.UI.Button button, Action callback, bool supportGamepadButtonsNav = true)
	{
		base.BindListener(button, callback, supportGamepadButtonsNav);
		getButtonImage(button)?.gameObject.SetActive(false);
	}

	static HashSet<string> IgnorePanelTypes = new HashSet<string>(new[]
	{
		"CommonTipsUIPanel"
	});
	private bool initialized;

	public override void Update()
	{
		base.Update();

		if (!initialized)
		{
			selectSystemButton();
			initialized = true;
		}

		if (Compass != null )
		{
			Compass.gameObject.SetActive(LevelMaster.Instance.IsInWorldMap && Jyx2LuaBridge.HaveItem(182));
			if (Compass.gameObject.activeSelf)
			{
				var p = LevelMaster.Instance.GetPlayerPosition();
				var pString = (p.x + 242).ToString("F0") + "," + (p.z + 435).ToString("F0");
				if (!LevelMaster.Instance.GetPlayer().IsOnBoat)
				{
					var b = LevelMaster.Instance.GetPlayer().GetBoatPosition();
					pString += "(" + (b.x + 242).ToString("F0") + "," + (b.z + 435).ToString("F0") + ")";
				}
				Compass.text = pString;
			}
		}
		
	}

	protected override void OnShowPanel(params object[] allParams)
	{
		base.OnShowPanel(allParams);
		RefreshNameMapName();
		RefreshDynamic();
	}

	private void selectSystemButton()
	{
		var systemButtonIndex = activeButtons.ToList().IndexOf(SystemButton_Button);
		if (systemButtonIndex > -1)
		{
			changeCurrentSelection(systemButtonIndex);
		}
	}

	protected override void changeCurrentSelection(int num)
	{
		base.changeCurrentSelection(num);
		for (var i = 0; i < activeButtons.Length; i++)
		{
			var button = activeButtons[i];
			getButtonImage(button)?.gameObject.SetActive(i == num);
		}
	}

	void RefreshDynamic()
	{
		RoleInstance role = GameRuntimeData.Instance.Player;
		string expText = string.Format("EXP:{0}/{1}", role.Exp, role.GetLevelUpExp());
		Exp_Text.text = expText;
		Level_Text.text = role.Level.ToString();
	}

	void RefreshNameMapName()
	{
		RoleInstance role = GameRuntimeData.Instance.Player;
		Name_Text.text = role.Name;

		var map = LevelMaster.GetCurrentGameMap();
		if (map != null)
		{
			MapName_Text.text = map.GetShowName();

			//var rt = Image_Right.GetComponent<RectTransform>();
			//rt.sizeDelta = new Vector2(isWorldMap?480:640, 100);
		}
	}

	async void OnXiakeBtnClick()
	{
		await Jyx2_UIManager.Instance.ShowUIAsync(nameof(XiakeUIPanel), GameRuntimeData.Instance.Player, GameRuntimeData.Instance.GetTeam().ToList());
	}

	async void OnBagBtnClick()
	{
		await Jyx2_UIManager.Instance.ShowUIAsync(nameof(BagUIPanel), GameRuntimeData.Instance.AllRoles[0].Items, new Action<String>(OnUseItem));
	}

	//使用物品
	async void OnUseItem(String id)
	{
		if (id == null) return;
		//同物品的不同实例 id相同的问题 todo
		var item =  GameRuntimeData.Instance.AllRoles[0].GetItem(id); //注意allroles中主角放第一个
		if (item == null)
		{
			Debug.LogError("use item error, id=" + id);
			return;
		}
		//剧情类不能使用
		if ((int)item.ItemType == 0)
		{
			GameUtil.DisplayPopinfo("此道具不能在此使用");
			return;
		}

		var runtime = GameRuntimeData.Instance;

		async void Action()
		{
			async void Callback(RoleInstance selectRole)
			{
				if (selectRole == null) return;

				if (selectRole.Id == runtime.GetItemUser(item)) return;

				if (selectRole.CanUseItem(id))
				{
					//武器
					if ((int)item.ItemType == 10)
					{
						if (runtime.GetItemUser(item) != -1)
						{
							RoleInstance roleInstance = runtime.GetRoleInTeam(runtime.GetItemUser(item));
							roleInstance.UnequipItem(roleInstance.Equipments[0],0);
						}

						selectRole.UnequipItem(selectRole.Equipments[0],0);
						selectRole.UseItem(selectRole.Equipments[0]);
						runtime.SetItemUser(item.Id, selectRole.Id);
						GameUtil.DisplayPopinfo($"{selectRole.Name}使用了{item.Name}");
					}
					//防具
					else if ((int)item.ItemType == 11)
					{
						if (runtime.GetItemUser(item) != -1)
						{
							RoleInstance roleInstance = runtime.GetRoleInTeam(runtime.GetItemUser(item));
							roleInstance.UnequipItem(roleInstance.Equipments[1],1);
						}
						selectRole.UnequipItem(selectRole.Equipments[1],1);
						selectRole.UseItem(selectRole.Equipments[1]);
						runtime.SetItemUser(item.Id, selectRole.Id);
						GameUtil.DisplayPopinfo($"{selectRole.Name}使用了{item.Name}");
					}
					//药品
					else if ((int)item.ItemType == 3)
					{
						selectRole.UseItem(item);
						runtime.AllRoles[0].AlterItem(item.ConfigId, -1,item.Quality);
						GameUtil.DisplayPopinfo($"{selectRole.Name}使用了{item.Name}");
					}
				}
				else
				{
					GameUtil.DisplayPopinfo((int)item.ItemType == 1 ? "此人不适合配备此物品" : "此人不适合修炼此物品");
					return;
				}
			}

			await GameUtil.SelectRole(runtime.GetTeam(), Callback);
		}

		await GameUtil.ShowYesOrNoUseItem(item, Action);

	}

	async void OnSystemBtnClick()
	{
		SettingsPanel.gameObject.SetActive(true);
		var levelMaster = LevelMaster.Instance;
		levelMaster.SetPlayerCanController(false);
		levelMaster.StopPlayerNavigation();
	}
	async void OnCloseBtnClick()
	{
		SettingsPanel.gameObject.SetActive(false);
		var levelMaster = LevelMaster.Instance;
		levelMaster.SetPlayerCanController(true);
	}
	async void OnSaveBtnClick()
	{
		await Jyx2_UIManager.Instance.ShowUIAsync(nameof(SavePanel), new Action<int>((index) =>
		{
			var levelMaster = FindObjectOfType<LevelMaster>();
			levelMaster.OnManuelSave(index);
		}), "选择存档位".GetContent(nameof(MainUIPanel)));
	}
	
	async void OnLoadBtnClick()
	{
		await Jyx2_UIManager.Instance.ShowUIAsync(nameof(SavePanel), new Action<int>((index) =>
		{
			GameRuntimeData.DoLoadGame(index);
		}), "选择读档位".GetContent(nameof(MainUIPanel)));
	}
	
	async void OnMainMenuBtnClick()
	{
		List<string> selectionContent = new List<string>() { "是(Y)", "否(N)" };
		await Jyx2_UIManager.Instance.ShowUIAsync(nameof(ChatUIPanel), ChatType.Selection, "0", "将丢失未保存进度，是否继续？", selectionContent, new Action<int>((index) =>
		{
			if (index == 0)
			{
				LoadingPanel.Create(null).Forget();
			}
		}));
	}
	
	async void OnSettingsBtnClick()
	{
		await Jyx2_UIManager.Instance.ShowUIAsync(nameof(GameSettingsPanel));
	}
	

	public void DoShowAnimator()
	{
		//AnimRoot_RectTransform.anchoredPosition = new Vector2(0, 150);
		//AnimRoot_RectTransform.DOAnchorPosY(-50, 1.0f);
	}

	public void DoHideAnimator()
	{

	}

	private void OnEnable()
	{
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.Escape, () =>
		{
			if (LevelMaster.Instance.IsPlayerCanControl())
			{
				OnSystemBtnClick();
			}
		});
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.X, () =>
		{
			if (LevelMaster.Instance.IsPlayerCanControl())
			{
				OnXiakeBtnClick();
			}
		});
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.B, () =>
		{
			if (LevelMaster.Instance.IsPlayerCanControl())
			{
				OnBagBtnClick();
			}
		});
	}

	private void OnDisable()
	{
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.Escape);
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.X);
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.B);
	}

	protected override bool captureGamepadAxis => true;

	protected override void OnDirectionalDown()
	{
		//do nothing
	}

	protected override void OnDirectionalUp()
	{
		//do nothing
	}

	protected override void OnDirectionalLeft()
	{
		base.OnDirectionalUp();
	}

	protected override void OnDirectionalRight()
	{
		base.OnDirectionalDown();
	}

	protected override string confirmButtonName()
	{
		return GamepadHelper.START_BUTTON;
	}

	//don't reset to 0 for this main, since it will select the system button automatically
	protected override bool resetCurrentSelectionOnShow => false;
}
