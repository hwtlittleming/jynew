
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class BattleMainUIPanel
{
	private RectTransform BattleHpRoot_RectTransform;
	public static RectTransform DialogRoot_RectTransform;
	public static RectTransform StateRoot_RectTransform;
	private RectTransform CurrentRole_RectTransform;
	private Text NameText_Text;
	private Text DetailText_Text;
	private Image PreImage_Image;
	private RectTransform HUDItem_RectTransform;

	public void InitTrans()
	{
		BattleHpRoot_RectTransform = transform.Find("BattleHpRoot").GetComponent<RectTransform>();
		DialogRoot_RectTransform = transform.Find("DialogRoot").GetComponent<RectTransform>();
		StateRoot_RectTransform = transform.Find("StateRoot").GetComponent<RectTransform>();
		CurrentRole_RectTransform = transform.Find("CurrentRole").GetComponent<RectTransform>();
		NameText_Text = transform.Find("CurrentRole/NameText").GetComponent<Text>();
		DetailText_Text = transform.Find("CurrentRole/DetailText").GetComponent<Text>();
		PreImage_Image = transform.Find("CurrentRole/HeadAvataPre/Mask/PreImage").GetComponent<Image>();
		HUDItem_RectTransform = transform.Find("Prefabs/HUDItem").GetComponent<RectTransform>();

	}
}
