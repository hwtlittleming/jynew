
using UnityEngine;
using Jyx2;
using System;
using System.Collections;
using i18n.TranslatorDef;
using Jyx2.Middleware;
using UnityEngine.UI;

using Configs;
using Cysharp.Threading.Tasks;

public partial class GameMainMenu : UIBase
{

	private enum PanelType
	{
		Home,
		NewGamePage,
		PropertyPage,
		LoadGamePage,
	}
	private RandomPropertyComponent m_randomProperty;

	private PanelType m_panelType;

	private int main_menu_index => current_selection;

	private const int NewGameIndex = 0;
	private const int LoadGameIndex = 1;
	private const int SettingsIndex = 2;
	private const int QuitGameIndex = 3;

	async void OnStart()
	{
		MainMenuTitles.SetActive(false);
		//显示loading
		var c = StartCoroutine(ShowLoading());
		await BeforeSceneLoad.loadFinishTask;
		StopCoroutine(c);
		LoadingText.gameObject.SetActive(false);
		homeBtnAndTxtPanel_RectTransform.gameObject.SetActive(true);

		JudgeShowReleaseNotePanel();
	}

	void JudgeShowReleaseNotePanel()
	{
		//每个更新显示一次
		string key = "RELEASENOTE_" + Application.version;
		if (!PlayerPrefs.HasKey(key))
		{
			ReleaseNote_Panel.gameObject.SetActive(true);
			PlayerPrefs.SetInt(key, 1);
			PlayerPrefs.Save();
		}
	}

	IEnumerator ShowLoading()
	{
		while (true)
		{
			LoadingText.gameObject.SetActive(!LoadingText.gameObject.activeSelf);
			yield return new WaitForSeconds(0.5f);
		}
	}


	public override UILayer Layer { get => UILayer.MainUI; }
	protected override void OnCreate()
	{
		InitTrans();
		RegisterEvent();
		m_randomProperty = this.StartNewRolePanel_RectTransform.GetComponent<RandomPropertyComponent>();
	}



	protected override Color normalButtonColor()
	{
		return ColorStringDefine.main_menu_normal;
	}

	protected override Color selectedButtonColor()
	{
		return ColorStringDefine.main_menu_selected;
	}

	protected override bool captureGamepadAxis
	{
		get { return true; }
	}


	protected override void handleGamepadButtons()
	{
		if (m_panelType != PanelType.NewGamePage
			&& m_panelType != PanelType.LoadGamePage
			&& m_panelType != PanelType.PropertyPage
			&& !ReleaseNote_Panel.gameObject.activeSelf)
			base.handleGamepadButtons();
		else
		{
			if (gameObject.activeSelf)
				if (GamepadHelper.IsConfirm())
				{
					if (m_panelType == PanelType.NewGamePage)
					{
						OnCreateBtnClicked();
					}
					else if (m_panelType == PanelType.PropertyPage)
					{
						OnCreateRoleYesClick();
					}
				}
				else if (GamepadHelper.IsCancel())
				{
					if (m_panelType == PanelType.NewGamePage
						|| m_panelType == PanelType.LoadGamePage) //save/ load panel has its own logic to close/ hide themself
					{
						OnBackBtnClicked();
					}
					else if (m_panelType == PanelType.PropertyPage)
					{
						OnCreateRoleNoClick();
					}
					else if (ReleaseNote_Panel.gameObject.activeSelf)
					{
						ReleaseNote_Panel.gameObject.SetActive(false);
					}
				}
		}
	}



	protected override void OnShowPanel(params object[] allParams)
	{
		base.OnShowPanel(allParams);
		OnStart();
		AudioManager.PlayMusic(16);
		m_panelType = PanelType.Home;
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.DownArrow, () =>
		{
			OnDirectionalDown();
		});

		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.UpArrow, () =>
		{
			OnDirectionalUp();
		});
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.Space, () =>
		{
			onButtonClick();
		});
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.Escape, () =>
		{
			if (m_panelType == PanelType.NewGamePage || m_panelType == PanelType.LoadGamePage)//save/ load panel has its own logic to close/ hide themself
			{
				OnBackBtnClicked();
			}
		});
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.Return, () =>
		{
			if (m_panelType == PanelType.NewGamePage)
			{
				onButtonClick(); //OnCreateBtnClicked();
			}
		});
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.Y, () =>
		{
			if (m_panelType == PanelType.PropertyPage)
			{
				OnCreateRoleYesClick();
			}
		});
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.N, () =>
		{
			if (m_panelType == PanelType.PropertyPage)
			{
				OnCreateRoleNoClick();
			}
		});
	}

	private void toggleButtonOutline(Button button, bool on)
	{
		var outline = button?.gameObject.GetComponentInChildren<Outline>();
		if (outline != null)
			outline.enabled = on;
	}

	int current_selection_x = 3;

	private void selectBottomButton(int index)
	{
		current_selection_x = index;
		isXSelection = index > -1;

		for (var i = 0; i < bottomButtons.Count; i++)
		{
			var button = bottomButtons[i];
			toggleButtonOutline(button, i == current_selection_x);
		}

		if (index > -1)
			changeCurrentSelection(-1);
	}

	private void onButtonClick()
	{
		if (m_panelType == PanelType.Home)
		{
			if (main_menu_index == NewGameIndex)
			{
				OnNewGameClicked();
			}
			else if (main_menu_index == LoadGameIndex)
			{
				OnLoadGameClicked();
			}else if (main_menu_index == SettingsIndex)
			{
				OpenSettingsPanel();
			}
			else if (main_menu_index == QuitGameIndex)
			{
				OnQuitGameClicked();
			}
		}
	}

	public void OnNewGameClicked()
	{
		transform.Find("mainPanel/ExtendPanel")?.gameObject.SetActive(false); 
		OnNewGame();
	}

	// merge to SavePanel.cs
	public async void OnLoadGameClicked()
	{
		m_panelType = PanelType.LoadGamePage;

		await UIManager.Instance.ShowUIAsync(nameof(SavePanel), new Action<int>((index) =>
		{
			if (!GameRuntimeData.DoLoadGame(index) && m_panelType == PanelType.LoadGamePage)
			{
				OnNewGame();
			}
		}), "选择读档位".GetContent(nameof(GameMainMenu)), new Action(() =>
		 {
			 m_panelType = PanelType.Home;
		 }));
		//---------------------------------------------------------------------------
		//---------------------------------------------------------------------------
	}

	public void OnQuitGameClicked()
	{
		Application.Quit();
	}

	public void OnCreateBtnClicked()
	{
		string newName = this.NameInput_InputField.text;

		//todo:去掉特殊符号
		if (string.IsNullOrWhiteSpace(newName))
			return;

		m_panelType = PanelType.PropertyPage;
		//todo:给玩家提示
		RoleInstance role = GameRuntimeData.Instance.Player;
		role.Name = newName;

		this.InputNamePanel_RectTransform.gameObject.SetActive(false);
		this.StartNewRolePanel_RectTransform.gameObject.SetActive(true);
		m_randomProperty.ShowComponent();
		// generate random property at randomP panel first show
		// added by eaphone at 2021/05/23
		OnCreateRoleNoClick();
	}

	//游戏开始界面的 开始游戏
	void OnNewGame()
	{
		var runtime = GameRuntimeData.CreateNew();

		m_panelType = PanelType.NewGamePage;
		this.homeBtnAndTxtPanel_RectTransform.gameObject.SetActive(false);
		this.InputNamePanel_RectTransform.gameObject.SetActive(true);
		NameInput_InputField.ActivateInputField();
	}

	private void RegisterEvent()
	{
		BindListener(this.NewGameButton_Button, OnNewGameClicked);
		BindListener(this.LoadGameButton_Button, OnLoadGameClicked);
		BindListener(this.SettingsButton_Button, OpenSettingsPanel);
		BindListener(this.QuitGameButton_Button, OnQuitGameClicked);
		
		BindListener(this.inputSure_Button, OnCreateBtnClicked, false);
		BindListener(this.inputBack_Button, OnBackBtnClicked, false);
		BindListener(this.YesBtn_Button, OnCreateRoleYesClick, false);
		BindListener(this.NoBtn_Button, OnCreateRoleNoClick, false);
	}
	private void OnCreateRoleYesClick()
	{
		//reset mode, fix bug or quit game and new game again on main menu goes straight to property panel
		m_panelType = PanelType.Home;
		this.homeBtnAndTxtPanel_RectTransform.gameObject.SetActive(true);
		this.StartNewRolePanel_RectTransform.gameObject.SetActive(false);
		var loadPara = new LevelMaster.LevelLoadPara();
		loadPara.loadType = LevelMaster.LevelLoadPara.LevelLoadType.StartAtTrigger;
		loadPara.triggerName = "0";
		GameRuntimeData.Instance.startDate = DateTime.Now;
		//加载地图
		var startMap = ConfigMap.GetGameStartMap();
		
		LevelLoader.LoadGameMap(startMap, loadPara, () =>
		{
			//首次进入游戏音乐
			AudioManager.PlayMusic(GameConst.GAME_START_MUSIC_ID);
			UIManager.Instance.HideUI(nameof(GameMainMenu));

			LevelMaster.Instance.GetPlayer().transform.rotation = Quaternion.Euler(Vector3.zero);
		});
	}
	private void OnCreateRoleNoClick()
	{
		RoleInstance role = GameRuntimeData.Instance.Player;
		for (int i = 0; i <= 12; i++)
		{
			GenerateRamdomPro(role, i);
		}
		GenerateRamdomPro(role, 25);//资质
		m_randomProperty.RefreshProperty();
	}

	private void GenerateRamdomPro(RoleInstance role, int i)
	{
		string key = i.ToString();
		if (GameConst.ProItemDic.ContainsKey(key))
		{
			PropertyItem item = GameConst.ProItemDic[key];
			int value = Tools.GetRandomInt(item.DefaulMin, item.DefaulMax);
			role.GetType().GetField(item.PropertyName).SetValue(role, value);
		}
	}

	private void OnBackBtnClicked()
	{
		this.homeBtnAndTxtPanel_RectTransform.gameObject.SetActive(true);
		this.InputNamePanel_RectTransform.gameObject.SetActive(false);
		m_panelType = PanelType.Home;
		
		transform.Find("mainPanel/ExtendPanel")?.gameObject.SetActive(true);
	}

	protected override void OnHidePanel()
	{
		base.OnHidePanel();
		//释放资源
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.DownArrow);
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.UpArrow);
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.Space);
		//GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.Escape);
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.Return);
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.Y);
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.N);
	}

	public void OnOpenURL(string url)
	{
		Tools.openURL(url);
	}

	/// <summary>
	/// 打开设置界面
	/// </summary>
	public void OpenSettingsPanel()
	{
		UIManager.Instance.ShowUIAsync(nameof(GameSettingsPanel)).Forget();
	}

	bool isXSelection = false;

	protected override void OnDirectionalLeft()
	{
		var nextSelectionX = (current_selection_x <= 0) ? bottomButtons.Count - 1 : current_selection_x - 1;

		selectBottomButton(nextSelectionX);
	}

	protected override void OnDirectionalRight()
	{
		var nextSelectionX = (current_selection_x >= bottomButtons.Count - 1) ? 0 : current_selection_x + 1;

		selectBottomButton(nextSelectionX);
	}

	protected override void changeCurrentSelection(int num)
	{
		base.changeCurrentSelection(num);

		if (num > -1)
			selectBottomButton(-1);
	}

	protected override void buttonClickAt(int position)
	{
		if (!isXSelection)
			base.buttonClickAt(position);
		else
		{
			bottomButtons[current_selection_x]?.onClick?.Invoke();
		}
	}

}
