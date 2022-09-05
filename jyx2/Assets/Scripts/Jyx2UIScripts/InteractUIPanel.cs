/*
 * 金庸群侠传3D重制版
 * https://github.com/jynew/jynew
 *
 * 这是本开源项目文件头，所有代码均使用MIT协议。
 * 但游戏内资源和第三方插件、dll等请仔细阅读LICENSE相关授权协议文档。
 *
 * 金庸老先生千古！
 */
using System;
using UnityEngine;

public partial class InteractUIPanel : Jyx2_UIBase
{
	public override UILayer Layer => UILayer.NormalUI;

	Action m_callback1;
	Action m_callback2;
	Action m_callback3;
	Action m_callback4;
	private int buttonCount;
	private float lastDpadY;
	private int focusButtonPos = 0;

	protected override void OnCreate()
	{
		InitTrans();

		BindListener(MainBg_Button1, () => OnBtnClick(0), false);
		BindListener(MainBg_Button2, () => OnBtnClick(1), false);
		BindListener(MainBg_Button3, () => OnBtnClick(2), false);
		BindListener(MainBg_Button4, () => OnBtnClick(3), false);
	}

	protected override void OnShowPanel(params object[] allParams)
	{
		base.OnShowPanel(allParams);

		if (allParams == null) return;

		//后改更灵活的写法
		if (allParams.Length == 2)
		{
			MainText_Text1.text = allParams[0] as string;
			m_callback1 = allParams[1] as Action;
			MainBg_Button2.gameObject.SetActive(false);
			MainBg_Button3.gameObject.SetActive(false);
			MainBg_Button4.gameObject.SetActive(false);
		}
		else if (allParams.Length == 4)
		{
			MainText_Text1.text = allParams[0] as string;
			m_callback1 = allParams[1] as Action;
			MainBg_Button2.gameObject.SetActive(true);
			MainBg_Button3.gameObject.SetActive(false);
			MainBg_Button4.gameObject.SetActive(false);
			MainText_Text2.text = allParams[2] as string;
			m_callback2 = allParams[3] as Action;
		}
		else if (allParams.Length == 6)
		{
			MainText_Text1.text = allParams[0] as string;
			m_callback1 = allParams[1] as Action;
			MainBg_Button2.gameObject.SetActive(true);
			MainText_Text2.text = allParams[2] as string;
			m_callback2 = allParams[3] as Action;
			MainBg_Button3.gameObject.SetActive(true);
			MainBg_Button4.gameObject.SetActive(false);
			MainText_Text3.text = allParams[4] as string;
			m_callback3 = allParams[5] as Action;
		}
		else if (allParams.Length == 8)
		{
			MainText_Text1.text = allParams[0] as string;
			m_callback1 = allParams[1] as Action;
			MainBg_Button2.gameObject.SetActive(true);
			MainText_Text2.text = allParams[2] as string;
			m_callback2 = allParams[3] as Action;
			MainBg_Button3.gameObject.SetActive(true);
			MainText_Text3.text = allParams[4] as string;
			m_callback3 = allParams[5] as Action;
			MainBg_Button4.gameObject.SetActive(true);
			MainText_Text4.text = allParams[6] as string;
			m_callback4 = allParams[7] as Action;
		}
	}

	void OnBtnClick(int buttonIndex)
	{
		Action temp = m_callback1;
		if ( buttonIndex == 0 )
		{
			temp = m_callback1;
		}else if ( buttonIndex == 1 )
		{
			temp = m_callback2;
		}else if ( buttonIndex == 2 )
		{
			temp = m_callback3;
		}else if ( buttonIndex == 3 )
		{
			temp = m_callback4;
		}

		Jyx2_UIManager.Instance.HideUI(nameof(InteractUIPanel));
		temp?.Invoke();
	}

	protected override void handleGamepadButtons()
	{
		if (gameObject.activeSelf)
			if (LevelMaster.Instance?.IsPlayerCanControl() ?? true)
			{
				if (Input.GetKeyDown(KeyCode.Space) || GamepadHelper.IsConfirm())
				{
					OnBtnClick(0);
				}
				else if (Input.GetKeyDown(KeyCode.Return) || GamepadHelper.IsCancel())
				{
					OnBtnClick(1);
				}
				else if (Input.GetKeyDown(KeyCode.Escape) || GamepadHelper.IsJump())
				{
					Jyx2_UIManager.Instance.HideUI(nameof(InteractUIPanel));
				}
			}
	}

	protected override void OnHidePanel()
	{
		base.OnHidePanel();
		m_callback1 = null;
		m_callback2 = null;
		m_callback3 = null;
		m_callback4 = null;
	}
}
