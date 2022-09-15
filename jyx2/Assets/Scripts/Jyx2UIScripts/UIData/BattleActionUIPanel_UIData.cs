
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class BattleActionUIPanel
{
	private RectTransform LeftActions_RectTransform;
	private RectTransform RightActions_RectTransform;
	private Button Defend_Button;
	private Button Save_Button;
	private Button Catch_Button;
	private Button Auto_Button;
	
	private Button Strategy_Button;
	private Button Item_Button;


	private RectTransform Skills_RectTransform;
	private RectTransform SkillItem_RectTransform;
	private Button NormalAttack_Button;

	public void InitTrans()
	{
		LeftActions_RectTransform = transform.Find("LeftActions").GetComponent<RectTransform>();
		RightActions_RectTransform = transform.Find("RightActions").GetComponent<RectTransform>();
		
		Defend_Button = transform.Find("RightActions/Defend").GetComponent<Button>();
		Save_Button = transform.Find("RightActions/Save").GetComponent<Button>();
		Catch_Button = transform.Find("RightActions/Catch").GetComponent<Button>();
		Auto_Button = transform.Find("RightActions/Auto").GetComponent<Button>();

		Strategy_Button = transform.Find("LeftActions/Strategy").GetComponent<Button>();
		Item_Button = transform.Find("LeftActions/Item").GetComponent<Button>();
		
		Skills_RectTransform = transform.Find("Skills").GetComponent<RectTransform>();
		SkillItem_RectTransform = transform.Find("Prefabs/SkillItem").GetComponent<RectTransform>();
		NormalAttack_Button = transform.Find("NormalAttack").GetComponent<Button>();

	}
}
