
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class GameMainMenu
{
	private RectTransform mainPanel_RectTransform;
	private RectTransform homeBtnAndTxtPanel_RectTransform;
	private Button NewGameButton_Button;
	private Button LoadGameButton_Button;
	private Button SettingsButton_Button;
	private Button QuitGameButton_Button;
	private RectTransform SavePanel_RectTransform;
	private RectTransform savePanelContainer_RectTransform;
	private Button BackButton_Button;
	private RectTransform InfoPanel_RectTransform;
	private RectTransform InputNamePanel_RectTransform;
	private InputField NameInput_InputField;
	private Button inputSure_Button;
    private Button inputBack_Button;
    private RectTransform StartNewRolePanel_RectTransform;
	private Button NoBtn_Button;
	private Button YesBtn_Button;
	private RectTransform PropertyItem_RectTransform;
	private RectTransform PropertyRoot_RectTransform;
	private Text LoadingText;
	private ReleaseNotePanel ReleaseNote_Panel;
	private GameObject MainMenuTitles;

	List<Button> bottomButtons = new List<Button>();

	public void InitTrans()
	{
		mainPanel_RectTransform = transform.Find("mainPanel").GetComponent<RectTransform>();
		homeBtnAndTxtPanel_RectTransform = transform.Find("mainPanel/homeBtnAndTxtPanel").GetComponent<RectTransform>();
		NewGameButton_Button = transform.Find("mainPanel/homeBtnAndTxtPanel/NewGameButton").GetComponent<Button>();
		LoadGameButton_Button = transform.Find("mainPanel/homeBtnAndTxtPanel/LoadGameButton").GetComponent<Button>();
		SettingsButton_Button = transform.Find("mainPanel/homeBtnAndTxtPanel/GameSettingsButton").GetComponent<Button>();
		QuitGameButton_Button = transform.Find("mainPanel/homeBtnAndTxtPanel/QuitGameButton").GetComponent<Button>();
		SavePanel_RectTransform = transform.Find("SavePanel").GetComponent<RectTransform>();
		savePanelContainer_RectTransform = transform.Find("SavePanel/savePanelContainer").GetComponent<RectTransform>();
		BackButton_Button = transform.Find("SavePanel/BackButton").GetComponent<Button>();
		InfoPanel_RectTransform = transform.Find("InfoPanel").GetComponent<RectTransform>();
		InputNamePanel_RectTransform = transform.Find("InputNamePanel").GetComponent<RectTransform>();
		NameInput_InputField = transform.Find("InputNamePanel/NameInput").GetComponent<InputField>();
		inputSure_Button = transform.Find("InputNamePanel/inputSure").GetComponent<Button>();
        inputBack_Button = transform.Find("InputNamePanel/inputBack").GetComponent<Button>();
        StartNewRolePanel_RectTransform = transform.Find("StartNewRolePanel").GetComponent<RectTransform>();
		NoBtn_Button = transform.Find("StartNewRolePanel/NoBtn").GetComponent<Button>();
		YesBtn_Button = transform.Find("StartNewRolePanel/YesBtn").GetComponent<Button>();
		PropertyItem_RectTransform = transform.Find("StartNewRolePanel/PropertyItem").GetComponent<RectTransform>();
		PropertyRoot_RectTransform = transform.Find("StartNewRolePanel/PropertyRoot").GetComponent<RectTransform>();
		LoadingText = transform.Find("mainPanel/LoadingText").GetComponent<Text>();
		ReleaseNote_Panel = transform.Find("ReleaseNotePanel").GetComponent<ReleaseNotePanel>();
		MainMenuTitles = transform.Find("BG").gameObject;
		
		
		//bottom buttons
		foreach (Transform child in transform.Find("mainPanel/ExtendPanel"))
		{
			var btn = child.GetComponent<Button>();
			bottomButtons.Add(btn);
		}
	}
}
