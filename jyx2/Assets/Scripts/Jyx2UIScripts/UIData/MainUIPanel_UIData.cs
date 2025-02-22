
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class MainUIPanel
{
	private RectTransform AnimRoot_RectTransform;
	private Text Date_Text;
	private Text Name_Text;
	private Text MapName_Text;
	private Button XiakeButton_Button;
	private Button BagButton_Button;
	private Button SystemButton_Button;
	private Image Image_Right;
	private Text Compass;

	//子一级菜单按钮
	private RectTransform SettingsPanel;
	private Button Save_Button;
	private Button Load_Button;
	private Button Close_Button;
	private Button Settings_Button;
	private Button MainMenu_Button;
	
	public void InitTrans()
	{
		AnimRoot_RectTransform = transform.Find("AnimRoot").GetComponent<RectTransform>();
		Date_Text = transform.Find("AnimRoot/PlayerStatus/Date").GetComponent<Text>();
		Name_Text = transform.Find("AnimRoot/PlayerStatus/Name").GetComponent<Text>();
		XiakeButton_Button = transform.Find("AnimRoot/BtnRoot/BtnRoot/XiakeButton").GetComponent<Button>();
		BagButton_Button = transform.Find("AnimRoot/BtnRoot/BtnRoot/BagButton").GetComponent<Button>();
		SystemButton_Button = transform.Find("AnimRoot/BtnRoot/BtnRoot/SystemButton").GetComponent<Button>();
		Image_Right = transform.Find("AnimRoot/BtnRoot/Image-right").GetComponent<Image>();
		Compass = transform.Find("AnimRoot/Compass/Text").GetComponent<Text>();
		MapName_Text = transform.Find("AnimRoot/PlayerStatus/MapName").GetComponent<Text>(); //显示当前地图名称
		
		SettingsPanel = transform.Find("SettingsPanel").GetComponent<RectTransform>();
		Save_Button = transform.Find("SettingsPanel/Container/SaveButton").GetComponent<Button>();
		Load_Button = transform.Find("SettingsPanel/Container/LoadButton").GetComponent<Button>();
		Close_Button = transform.Find("SettingsPanel/Container/CloseButton").GetComponent<Button>();
		Settings_Button = transform.Find("SettingsPanel/Container/SettingsButton").GetComponent<Button>();
		MainMenu_Button = transform.Find("SettingsPanel/Container/MainMenuButton").GetComponent<Button>();
	}
}
