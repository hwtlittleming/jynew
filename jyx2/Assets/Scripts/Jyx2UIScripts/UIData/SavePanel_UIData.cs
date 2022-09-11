
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class SavePanel
{
	private RectTransform SaveParent_RectTransform;
	private Button BackButton_Button;
	private Button SaveItem_Button;
	private Text Main_Text;
	private Button ImButton_Button;
	private Button ExButton_Button;
	[Header("标识是否在死亡页面")] 
	public bool IsInGameOverPage = false;

	public void InitTrans()
	{
		SaveParent_RectTransform = transform.Find("SaveParent").GetComponent<RectTransform>();
		BackButton_Button = transform.Find("MainText/BackButton").GetComponent<Button>();
		SaveItem_Button = transform.Find("ItemRoot/SaveItem").GetComponent<Button>();
		Main_Text = transform.Find("MainText").GetComponent<Text>();
		ImButton_Button = transform.Find("FileIO/Import").GetComponent<Button>();
		ExButton_Button = transform.Find("FileIO/Export").GetComponent<Button>();
	}
}
