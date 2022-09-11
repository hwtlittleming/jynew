
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class BattleActionUIPanel
{
	private RectTransform LeftActions_RectTransform;
	private Button Move_Button;
	private Button UsePoison_Button;
	private Button Depoison_Button;
	private Button Heal_Button;
	private Button Item_Button;
	private Button Wait_Button;
	private Button Rest_Button;
	private RectTransform Skills_RectTransform;
	private RectTransform SkillItem_RectTransform;
	private Button NormalAttack_Button;

	public void InitTrans()
	{
		LeftActions_RectTransform = transform.Find("LeftActions").GetComponent<RectTransform>();
		Move_Button = transform.Find("LeftActions/Move").GetComponent<Button>();
		UsePoison_Button = transform.Find("LeftActions/UsePoison").GetComponent<Button>();
		Depoison_Button = transform.Find("LeftActions/Depoison").GetComponent<Button>();
		Heal_Button = transform.Find("LeftActions/Heal").GetComponent<Button>();
		Item_Button = transform.Find("LeftActions/Item").GetComponent<Button>();
		Wait_Button = transform.Find("LeftActions/Wait").GetComponent<Button>();
		Rest_Button = transform.Find("LeftActions/Rest").GetComponent<Button>();
		Skills_RectTransform = transform.Find("Skills").GetComponent<RectTransform>();
		SkillItem_RectTransform = transform.Find("Prefabs/SkillItem").GetComponent<RectTransform>();
		NormalAttack_Button = transform.Find("NormalAttack").GetComponent<Button>();

	}
}
