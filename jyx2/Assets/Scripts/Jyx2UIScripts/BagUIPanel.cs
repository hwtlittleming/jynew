
using Jyx2.Middleware;

using Jyx2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using i18n.TranslatorDef;
using Jyx2Configs;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public partial class BagUIPanel : Jyx2_UIBase
{
	public override UILayer Layer => UILayer.NormalUI;

	Action<String> m_callback;
	List<ItemInstance> m_itemsData;
	Jyx2ItemUI m_selectItem;
	Func<ItemInstance, bool> m_filter = null;
	
	enum BagFilter
	{
		All = 0,
		Item,
		Cost,
		Equipment,
		Book,
	}

	private BagFilter _filter = BagFilter.All;

	protected override void OnCreate()
	{
		InitTrans();
		IsBlockControl = true;
		BindListener(UseBtn_Button, OnUseBtnClick, false);
		BindListener(CloseBtn_Button, OnCloseBtnClick, false);
	}


	private void OnEnable()
	{
		GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.Escape, OnCloseBtnClick);
	}

	private void OnDisable()
	{
		GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.Escape);
	}

	protected override void OnShowPanel(params object[] allParams)
	{
		base.OnShowPanel(allParams);
		m_itemsData = (List<ItemInstance>)allParams[0];
		if (allParams.Length > 1)
			m_callback = (Action<String>)allParams[1];
		if (allParams.Length > 2)
			m_filter = (Func<ItemInstance, bool>)allParams[2];
		
		//道具类型过滤器
		int index = 0;
		currentFilterIndex = 0;
		foreach (var btn in m_Filters)
		{
			btn.onClick.RemoveAllListeners();
			var index1 = index;
			btn.onClick.AddListener(() =>
			{
				changeFilter(index1);
			});
			index++;
		}

		_filter = BagFilter.All;
		RefreshFocusFilter();
		RefreshScroll();
	}

	private void changeFilter(int index1)
	{
		_filter = (BagFilter)(index1);
		RefreshFocusFilter();
		RefreshScroll();
	}

	List<Jyx2ItemUI> visibleItems = new List<Jyx2ItemUI>();

	void RefreshScroll()
	{
		visibleItems.Clear();
		HSUnityTools.DestroyChildren(ItemRoot_RectTransform);
		bool hasSelect = false;

		itemX = 0;
		itemY = 0;

		float itemHeight = 0;

		foreach (ItemInstance item in m_itemsData)
		{
			if (m_filter != null && m_filter(item) == false)
				continue;

			if (_filter == BagFilter.Item && (int)item.ItemType != 0) continue;
			if (_filter == BagFilter.Book && (int)item.ItemType != 2) continue;
			if (_filter == BagFilter.Cost && (int)item.ItemType != 3) continue;
			if (_filter == BagFilter.Equipment && (int)item.ItemType != 1) continue;

			//循环创建物品单元
			var itemUI = Jyx2ItemUI.Create(item);
			itemUI.transform.SetParent(ItemRoot_RectTransform);
			itemUI.transform.localScale = Vector3.one;
			var btn = itemUI.GetComponent<Button>();

			visibleItems.Add(itemUI);

			BindListener(btn, () =>
			{
				OnItemSelect(itemUI, false);
			});

			if (!hasSelect && GamepadHelper.GamepadConnected)
			{
				//select the first item
				m_selectItem = itemUI;
				hasSelect = true;
			}

			itemUI.Select(m_selectItem == itemUI);

			if (itemHeight == 0)
			{
				itemHeight = (itemUI.transform as RectTransform).rect.height;
			}
		}

		//setAreasHeightForItemCompleteView(itemHeight, new[]
		//{
		//	ItemsArea_ScrollRect.rectTransform(),
		//	ItemDes_RectTransform
		//}); 

		if (m_selectItem != null)
			scrollIntoView(ItemsArea_ScrollRect, m_selectItem.transform as RectTransform, 
				ItemRoot_GridLayout, 0);
		
		ShowItemDes();


	}

	void ShowItemDes()
	{
		if (m_selectItem == null)
		{
			UseBtn_Button.gameObject.SetActive(false);
			ItemDes_RectTransform.gameObject.SetActive(false);
			return;
		}

		ItemDes_RectTransform.gameObject.SetActive(true);
		UseBtn_Button.gameObject.SetActive(true);
		var item = GameRuntimeData.Instance.Player.GetItem(m_selectItem._id);
		DesText_Text.text = UIHelper.GetItemDesText(item);
	}

	void OnItemSelect(Jyx2ItemUI itemUI, bool scroll)
	{
		if (m_selectItem == itemUI)
			return;

		if (m_selectItem)
			m_selectItem.Select(false);
		m_selectItem = itemUI;
		m_selectItem.Select(true);

		ShowItemDes();

		if (scroll)
			scrollIntoView(ItemsArea_ScrollRect, m_selectItem.gameObject.transform as RectTransform, 
				ItemRoot_GridLayout, 0);

	}

	void OnCloseBtnClick()
	{
		if (m_callback != null)
		{
			m_callback(null);
		}
		
		Jyx2_UIManager.Instance.HideUI(nameof(BagUIPanel));
		visiblityToggle(false);
	}

	void OnUseBtnClick()
	{
		if (m_selectItem == null || m_callback == null)
			return;
		Action<String> call = m_callback;
		RefreshScroll();
		//Jyx2_UIManager.Instance.HideUI(nameof(BagUIPanel));
		call(m_selectItem._id);
	}

	protected override void OnHidePanel()
	{
		base.OnHidePanel();
		//m_selectItem = null;
		m_callback = null;
		m_filter = null;
		HSUnityTools.DestroyChildren(ItemRoot_RectTransform);
	}
	
	void RefreshFocusFilter()
	{
		foreach (var btn in m_Filters)
		{
			btn.GetComponent<Image>().color = Color.white;
		}

		int index = (int)_filter;

		//高亮的边框颜色等于文字颜色
		m_Filters[index].GetComponent<Image>().color = m_Filters[index].GetComponentInChildren<Text>().color;
	}

	#region 手柄支持代码

	protected override int axisReleaseDelay
	{
		get
		{
			return 200;
		}
	}


	protected override bool captureGamepadAxis
	{
		get
		{
			return true;
		}
	}

	private int itemX = 0;
	private int itemY = 0;

	protected override void changeCurrentSelection(int num)
	{
		if (num >= 0 && num < visibleItems.Count)
		{
			OnItemSelect(visibleItems[num], true);
		}
		else
		{
			if (m_selectItem)
				m_selectItem.Select(false);
		}
	}

	private int getSelectedItemIndex()
	{
		if (visibleItems.Count == 0)
			return -1;

		int horizontalItemsCount = getColCount();
		return itemY * horizontalItemsCount + itemX;
	}

	private int getColCount()
	{
		if (visibleItems.Count == 0)
			return 1;

		return (int)Math.Floor(ItemRoot_RectTransform.rect.width / visibleItems[0].rectTransform().rect.width);
	}

	private int getRowCount()
	{
		return (int)Math.Ceiling((float)visibleItems.Count / (float)getColCount());
	}

	bool goingUpward = false;

	protected override void OnDirectionalLeft()
	{
		goingUpward = true;
		if (itemX > 0)
			itemX--;

		else if (itemY > 0)
		{
			itemX = getColCount() - 1;
			OnDirectionalUp();
		}

		changeCurrentSelectionWithAxis();
	}

	private bool changeCurrentSelectionWithAxis()
	{
		var itemIndex = getSelectedItemIndex();
		var validMove = (itemIndex > -1 && itemIndex < visibleItems.Count);

		if (validMove)
			changeCurrentSelection(itemIndex);

		return validMove;
	}

	protected override void OnDirectionalUp()
	{
		goingUpward = true;
		if (itemY > 0)
			itemY--;

		if (!changeCurrentSelectionWithAxis())
			itemY++;
	}

	protected override void OnDirectionalRight()
	{
		goingUpward = false;
		if (itemX < getColCount() - 1)
		{
			itemX++;
			if (!changeCurrentSelectionWithAxis())
				itemX--;
		}
		else if (itemY < getRowCount() - 1)
		{
			itemX = 0;
			OnDirectionalDown();
		}
	}

	protected override void OnDirectionalDown()
	{
		goingUpward = false;
		if (itemY < getRowCount() - 1)
			itemY++;

		if (!changeCurrentSelectionWithAxis())
			itemY--;
	}

	private int currentFilterIndex = 0;

	protected override void handleGamepadButtons()
	{
		if (GamepadHelper.IsConfirm())
		{
			OnUseBtnClick();
		}
		else if (GamepadHelper.IsCancel())
		{
			OnCloseBtnClick();
		}
		else if (GamepadHelper.IsTabLeft())
		{
			if (currentFilterIndex == 0)
				currentFilterIndex = m_Filters.Count - 1;
			else
				currentFilterIndex--;

			changeFilter(currentFilterIndex);
		}
		else if (GamepadHelper.IsTabRight())
		{
			if (currentFilterIndex == m_Filters.Count - 1)
				currentFilterIndex = 0;
			else
				currentFilterIndex++;

			changeFilter(currentFilterIndex);
		}
	}

	#endregion
}
