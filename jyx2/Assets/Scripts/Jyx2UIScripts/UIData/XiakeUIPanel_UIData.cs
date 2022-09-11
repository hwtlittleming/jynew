
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class XiakeUIPanel
{
	private Image PreImage_Image;
	private Text NameText_Text;
	private Text InfoText_Text;
	private Text SkillText_Text;
	private RectTransform Equipments;
	private Button ButtonSelectWeapon_Button;
	private Button ButtonSelectArmor_Button;
	private Button ButtonSelectShoes_Button;
	private Button ButtonSelectTreasure_Button;
	private Button LeaveButton_Button;
	private RectTransform RoleParent_RectTransform;
	private Button BackButton_Button;

	public void InitTrans()
	{
		PreImage_Image = transform.Find("MainContent/HeadAvataPre/Mask/PreImage").GetComponent<Image>();
		NameText_Text = transform.Find("MainContent/HeadAvataPre/NameText").GetComponent<Text>();
		InfoText_Text = transform.Find("MainContent/InfoScroll/Viewport/InfoText").GetComponent<Text>();
		SkillText_Text = transform.Find("MainContent/SkillScroll/Viewport/SkillText").GetComponent<Text>();
		Equipments = transform.Find("MainContent/Equipment").GetComponent<RectTransform>();
		ButtonSelectWeapon_Button = transform.Find("MainContent/Equipment/ButtonSelectWeapon").GetComponent<Button>();
		ButtonSelectArmor_Button = transform.Find("MainContent/Equipment/ButtonSelectArmor").GetComponent<Button>();
		ButtonSelectShoes_Button = transform.Find("MainContent/Equipment/ButtonSelectShoes").GetComponent<Button>();
		ButtonSelectTreasure_Button = transform.Find("MainContent/Equipment/ButtonSelectTreasure").GetComponent<Button>();
		LeaveButton_Button = transform.Find("MainContent/LeaveButton").GetComponent<Button>();
		RoleParent_RectTransform = transform.Find("MainContent/RoleScroll/Viewport/RoleParent").GetComponent<RectTransform>();
		BackButton_Button = transform.Find("MainContent/BackButton").GetComponent<Button>();
	}
}
