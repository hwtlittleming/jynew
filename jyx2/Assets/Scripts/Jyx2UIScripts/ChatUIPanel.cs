
using Jyx2;

using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using Jyx2.MOD;
using NUnit.Framework;
using Sirenix.Utilities;

public enum ChatType
{
	None = -1,
	RoleId = 1,
	Selection = 2,
}
public partial class ChatUIPanel : UIBase, IUIAnimator
{
	public override UILayer Layer => UILayer.NormalUI;
	public override bool IsOnly => true;

	Action _callback;
	ChatType _currentShowType = ChatType.None;
	string _currentText;//存一下要显示的文字 当文字要显示的时候 用一个指针显示当前显示到的索引 分多次显示，点击显示接下来的
	int _currentShowIndex = 0;
	protected override void OnCreate()
	{
		InitTrans();

		StorySelectionItem_Button.gameObject.SetActive(false);
		MainBg_Button.onClick.AddListener(OnMainBgClick);

		InitPanelTrigger();
	}

	void OnMainBgClick()
	{
		if (_currentShowType == ChatType.None)
			return;
		if (_currentShowType == ChatType.Selection)
			return;
		ShowText();
	}

	protected override void OnShowPanel(params object[] allParams)
	{
		base.OnShowPanel(allParams);
		ChatType _type = (ChatType)allParams[0];
		_currentShowType = _type;
		_currentShowIndex = 0;
		switch (_type)
		{
			case ChatType.RoleId:
				Show((string)allParams[1], (string)allParams[2], allParams[3].ToString(), (Action)allParams[4]);
				break;
			case ChatType.Selection:
				ShowSelection((string)allParams[1], (string)allParams[2], (List<string>)allParams[3],allParams[4].ToString(), (Action<int>)allParams[5]);
				break;
		}

		//临时将触发按钮隐藏
		var panel = FindObjectOfType<InteractUIPanel>();
		if (panel != null && panel.gameObject.activeSelf)
		{
			_interactivePanel = panel.gameObject;
			panel.gameObject.SetActive(false);
		}
	}

	protected override void OnHidePanel()
	{
		base.OnHidePanel();
		MainContent_Text.text = "";
	}


	private GameObject _interactivePanel = null;
	private Action<int> selectionCallback;
	private int selectionContentCount = 0;

	//展示人物图片,姓名
	private async UniTask ShowCharacter(String headId,String talker = null)
	{
		if (!talker.IsNullOrWhitespace() && talker != "")
		{  //传了 talker  无图片的对话格式 用于不想配置的角色讲话
			NameTxt_Text.text =  talker;
			HeadAvataPre_RectTransform.gameObject.SetActive(false);
			kuang_RectTransform.gameObject.SetActive(false);
			RoleHeadImage_Image.gameObject.SetActive(false);
			Name_RectTransform.gameObject.SetActive(true);
			
			//姓名居左
			Name_RectTransform.anchorMax = new Vector2(1, 0); 
			Name_RectTransform.anchorMin = new Vector2(1, 0); 
			Name_RectTransform.pivot = new Vector2(1, 0); 
			Name_RectTransform.anchoredPosition = new Vector2(-1600, 280); 
			
			//框占整行
			Content_RectTransform.sizeDelta = new Vector2(0,280); 
			Content_RectTransform.offsetMax = new Vector2(0,280);
			Content_RectTransform.offsetMin = new Vector2(0,0);
		}
		else 
		{
			// 有图片 且根据角色ID修改左右位置
			HeadAvataPre_RectTransform.gameObject.SetActive(true);
			Name_RectTransform.gameObject.SetActive(true);
			kuang_RectTransform.gameObject.SetActive(true);
		
			//主角名
			if (headId == "0" && GameRuntimeData.Instance.Player != null)
			{
				NameTxt_Text.text =  GameRuntimeData.Instance.Player.Name;
			}
			//非主角从人物配置库找
			var role = GameConfigDatabase.Instance.Get<ConfigCharacter>(headId);
			NameTxt_Text.text =  role.Name;
			

			Content_RectTransform.anchoredPosition = headId == "0"  ? Vector3.zero : new Vector3(400, 0, 0);

			Content_RectTransform.sizeDelta = new Vector2(-400, 280);


			HeadAvataPre_RectTransform.anchorMax = headId == "0" ? Vector2.right : Vector2.zero;
			HeadAvataPre_RectTransform.anchorMin = headId == "0" ? Vector2.right : Vector2.zero;
			HeadAvataPre_RectTransform.pivot = headId == "0" ? Vector2.right : Vector2.zero;
			HeadAvataPre_RectTransform.anchoredPosition = Vector3.zero;

			kuang_RectTransform.anchorMax = headId == "0" ? Vector2.right : Vector2.zero;
			kuang_RectTransform.anchorMin = headId == "0" ? Vector2.right : Vector2.zero;
			kuang_RectTransform.pivot = headId == "0" ? Vector2.right : Vector2.zero;
			kuang_RectTransform.anchoredPosition = Vector3.zero;

			Name_RectTransform.anchorMax = headId == "0" ? Vector2.right : Vector2.zero;
			Name_RectTransform.anchorMin = headId == "0" ? Vector2.right : Vector2.zero;
			Name_RectTransform.pivot = headId == "0" ? Vector2.right : Vector2.zero;
			Name_RectTransform.anchoredPosition = new Vector2(headId == "0" ? -400 : 400, 280);
		

			var url = $"Assets/BuildSource/head/{headId}.png";
			RoleHeadImage_Image.gameObject.SetActive(true);
			RoleHeadImage_Image.LoadAsyncForget(role.GetPic());
		}
		
	}

	//根据对话框最大显示字符以及标点断句分段显示对话 by eaphone at 2021/6/12
	async void ShowText(String NameText = "")
	{
		if (_currentShowIndex >= _currentText.Length - 1)
		{
			UIManager.Instance.HideUI(nameof(ChatUIPanel));
			_callback?.Invoke();
			_callback = null;

			if (_interactivePanel)
			{
				_interactivePanel.SetActive(true);
				_interactivePanel = null;
			}

			return;
		}
		var finalS = _currentText;
		if (_currentText.Length > GameConst.MAX_CHAT_CHART_NUM)
		{
			int preIndex = _currentShowIndex;
			string[] sList = _currentText.Substring(preIndex, _currentText.Length - preIndex).Split(new char[] { '！', '？', '，', '　' }, StringSplitOptions.RemoveEmptyEntries);//暂时不对,'．'进行分割，不然对话中...都会被去除掉
			var tempIndex = 0;
			foreach (var i in sList)
			{
				var tempNum = i.Length + 1;//包含分隔符
				if (tempIndex + tempNum < GameConst.MAX_CHAT_CHART_NUM)
				{
					tempIndex += tempNum;
					_currentShowIndex += tempNum;
					continue;
				}
				break;
			}
			_currentShowIndex = Mathf.Clamp(_currentShowIndex, 0, _currentText.Length);
			finalS = _currentText.Substring(preIndex, _currentShowIndex - preIndex);
		}
		else
		{
			_currentShowIndex = _currentText.Length;
		}

		finalS = finalS + NameText;
		//打字机效果
		foreach (var cha in finalS.ToCharArray())
		{
			MainContent_Text.text += cha;
			await  UniTask.Delay(30);
		}
		
	}
	

	public void Show(String headId, string msg, String talker, Action callback)
	{
		_currentText = $"{msg}";
		_callback = callback;
		SelectionPanel_RectTransform.gameObject.SetActive(false);
		
		ShowCharacter(headId,talker).Forget();
		
        ShowText(); //talker有值则显示名称
	}

	protected override void handleGamepadButtons()
	{
		if (gameObject.activeSelf)
			if (GamepadHelper.IsConfirm())
			{
				if (selectionContentCount > 1)
				{
					UIManager.Instance.HideUI(nameof(ChatUIPanel));
					selectionCallback?.Invoke(0);
				}
				else
				{
					OnMainBgClick();
				}
			}
			else if (GamepadHelper.IsCancel())
			{
				if (selectionContentCount > 1)
				{
					UIManager.Instance.HideUI(nameof(ChatUIPanel));
					selectionCallback?.Invoke(1);
				}
			}
			else if (Input.GetKeyDown(KeyCode.Space))
				OnMainBgClick();
	}

	public void ShowSelection(string roleId, string msg, List<string> selectionContent,String talker, Action<int> callback)
	{
		ShowCharacter(roleId,talker).Forget();
		MainContent_Text.text = $"{msg}";

		selectionCallback = callback;
		selectionContentCount = selectionContent.Count;

		ClearChildren(Container_RectTransform.transform);
		for (int i = 0; i < selectionContent.Count; i++)
		{
			int currentIndex = i;
			Button selectionItem = Instantiate(StorySelectionItem_Button);
			selectionItem.gameObject.SetActive(true);
			selectionItem.transform.Find("Text").GetComponent<Text>().text = selectionContent[i];

			var image = getButtonImage(selectionItem);
			if (image != null)
			{
				image.gameObject.SetActive(GamepadHelper.GamepadConnected);
				Sprite[] iconSprite = getGamepadIconSprites(i);

				image.sprite = iconSprite?.FirstOrDefault();
			}
			selectionItem.transform.SetParent(Container_RectTransform, false);
			BindListener(selectionItem, delegate
			{
				UIManager.Instance.HideUI(nameof(ChatUIPanel));
				callback?.Invoke(currentIndex);
			}, false);
		}

		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.Y, () =>
		{
			UIManager.Instance.HideUI(nameof(ChatUIPanel));
			callback?.Invoke(0);
		});
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.N, () =>
		{
			UIManager.Instance.HideUI(nameof(ChatUIPanel));
			callback?.Invoke(1);
		});
		SelectionPanel_RectTransform.gameObject.SetActive(true);
	}

	public static Sprite[] getGamepadIconSprites(int i)
	{
		string iconPath;
		switch (i)
		{
			case 0:
				iconPath = "Assets/BuildSource/Gamepad/confirm.png";
				break;
			case 1:
				iconPath = "Assets/BuildSource/Gamepad/cancel.png";
				break;
			case 2:
				iconPath = "Assets/BuildSource/Gamepad/action.png";
				break;
			case 3:
				iconPath = "Assets/BuildSource/Gamepad/jump.png";
				break;
			default:
				iconPath = "";
				break;
		}

		var iconSprite = !string.IsNullOrWhiteSpace(iconPath) ?
			Addressables.LoadAssetAsync<Sprite[]>(iconPath)
				.Result : null;
		return iconSprite;
	}

	public void DoShowAnimator()
	{
		//Content_RectTransform.anchoredPosition = Vector2.zero;
		//allTweenList.Add(Content_RectTransform.DOAnchorPosY(130, 0.5f));
		//HeadAvataPre_RectTransform.anchoredPosition = new Vector2(-300, 300);
		//allTweenList.Add(HeadAvataPre_RectTransform.DOAnchorPosX(300, 0.5f));
	}

	public void DoHideAnimator()
	{

	}

	public void OnDisable()
	{
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.Y);
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.N);
	}

	private void InitPanelTrigger()
	{
		List<EventTrigger.Entry> entries = Panel_Trigger.triggers;
		for (int i = 0; i < entries.Count; i++)
		{
			if (entries[i].eventID == EventTriggerType.PointerClick)
			{
				entries[i].callback = new EventTrigger.TriggerEvent();
				entries[i].callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>((BaseEventData) =>
				{
					OnMainBgClick();
				}));
				break;
			}
		}
	}

	protected override Image getButtonImage(Button button)
	{
		return button.transform.Find("GamepadButtonIcon")?.GetComponent<Image>();
	}
}
