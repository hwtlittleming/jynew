
using Jyx2;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;

public partial class MainUIPanel : UIBase, IUIAnimator
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
			Compass.gameObject.SetActive(LevelMaster.Instance.IsInWorldMap && LuaBridge.HaveItem("182"));
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
		//todo 存档日期到keyvalue
		Date_Text.text = string.Format("第{0}天-{1}",GameRuntimeData.Instance.KeyValues.GetValueOrDefault("day","1"), GameRuntimeData.Instance.KeyValues.GetValueOrDefault("dayTime","晨")); 
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
		await UIManager.Instance.ShowUIAsync(nameof(XiakeUIPanel), GameRuntimeData.Instance.Player, GameRuntimeData.Instance.GetTeam().ToList());
	}

	async void OnBagBtnClick()
	{
		await UIManager.Instance.ShowUIAsync(nameof(BagUIPanel), GameRuntimeData.Instance.Player.Items, new Action<String>(OnUseItem));
	}

	//使用物品
	async void OnUseItem(String id)
	{
		if (id == null) return;
		var item =  GameRuntimeData.Instance.Player.GetItem(id); 
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

		async void Callback(RoleInstance selectRole)
		{
			if (selectRole == null) return;

			if (selectRole.Id == runtime.GetItemUser(item)) return;

			if (selectRole.CanUseItem(id))
			{
				ItemInstance item = runtime.Player.GetItem(id);
				if (item.isEquipment())
				{
					int index = (int) item.ItemType - 10;
					//新装备的上一持有者卸下
					if (item.UseRoleId != -1)
					{
						runtime.GetRoleInTeam(item.UseRoleId).UnequipItem(item,index);
					}

					//使用者原装备卸下
					ItemInstance yetItem =  selectRole.Equipments[index]; 
					if(yetItem!=null) selectRole.UnequipItem(yetItem,index);
					
					//使用者使用新装备
					selectRole.Equipments[index] = item;//替换存储的角色装备
					selectRole.UseItem(selectRole.Equipments[index]); //加减属性
					runtime.SetItemUser(item.Id, selectRole.Id);
				}
				//药品
				else if ((int)item.ItemType == 3)
				{
					selectRole.UseItem(item);
					runtime.Player.AlterItem(item.ConfigId, -1,item.Quality);
					GameUtil.DisplayPopinfo($"{selectRole.Name}使用了{item.Name}");
				}
			}
			else
			{
				GameUtil.DisplayPopinfo("此人不适合使用此物品");
				return;
			}
		}
		
		await GameUtil.SelectRole(runtime.GetTeam(), Callback);
		
	}

	async void OnSystemBtnClick()
	{
		var levelMaster = LevelMaster.Instance;
		if (SettingsPanel.gameObject.active == true)
		{
			SettingsPanel.gameObject.SetActive(false);
			levelMaster.SetPlayerCanController(true);
			return;
		}
		SettingsPanel.gameObject.SetActive(true);
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
		await UIManager.Instance.ShowUIAsync(nameof(SavePanel), new Action<int>((index) =>
		{
			var levelMaster = FindObjectOfType<LevelMaster>();
			levelMaster.OnManuelSave(index);
		}), "选择存档位".GetContent(nameof(MainUIPanel)));
	}
	
	async void OnLoadBtnClick()
	{
		await UIManager.Instance.ShowUIAsync(nameof(SavePanel), new Action<int>((index) =>
		{
			GameRuntimeData.DoLoadGame(index);
		}), "选择读档位".GetContent(nameof(MainUIPanel)));
	}
	
	async void OnMainMenuBtnClick()
	{
		List<string> selectionContent = new List<string>() { "是(Y)", "否(N)" };
		await UIManager.Instance.ShowUIAsync(nameof(ChatUIPanel), ChatType.Selection, "0", "将丢失未保存进度，是否继续？", selectionContent,new Action<int>((index) =>
		{
			if (index == 0)
			{
				LoadingPanel.Create(null).Forget();
			}
		}));
	}
	
	async void OnSettingsBtnClick()
	{
		await UIManager.Instance.ShowUIAsync(nameof(GameSettingsPanel));
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
			OnSystemBtnClick();
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
		//GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.Escape);
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
