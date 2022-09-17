
using Jyx2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//输入管理 主要是输入的一些共有方法
public class InputManager
{
    private static InputManager _instance;
    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new InputManager();
                _instance.Init();
            }
            return _instance;
        }
    }

    private void Init()
    {
    }


    bool IsPointerOverUIObjectExceptTouchpad()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        if (results == null || results.Count == 0)
            return false;

        //排除掉touchpad
        if (results.Count == 1)
        {
            var hit = results[0];
            if (hit.gameObject == LevelMaster.Instance.m_TouchPad.gameObject)
                return false;
        }

        return true;
    }

    //获取点击的格子对象信息
    public GameObject GetMouseUpBattleBlock()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)  ) //&& !IsPointerOverUIObjectExceptTouchpad() && !EventSystem.current.IsPointerOverGameObject()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if(hitInfo.transform.gameObject) return hitInfo.transform.gameObject;
            }
        }
        return null;
    }

    public BattleBlockData GetMouseOverBattleBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        //待调整为格子才可以移动
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, 1 << LayerMask.NameToLayer("Ground")))
        {
            var block = BattleboxHelper.Instance.GetLocationBattleBlock(hitInfo.point);
            if (block != null && block.IsActive && !block.Inaccessible)
            {
                return block;
            }
        }

        return null;
    }

    public BattleBlockData GetMouseDownBattleBlock()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObjectExceptTouchpad())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //待调整为格子才可以移动
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, 1 << LayerMask.NameToLayer("Ground")))
            {
                var block = BattleboxHelper.Instance.GetLocationBattleBlock(hitInfo.point);
                if (block != null && block.IsActive && !block.Inaccessible)
                {
                    return block;
                }
            }
        }
        return null;
    }
}
