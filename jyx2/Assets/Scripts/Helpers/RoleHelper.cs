

using Jyx2;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// TODO:不确定RoleHelper是否可以和MapRole直接合并，或者也没必要
/// </summary>
public static class RoleHelper
{
    /// <summary>
    /// 寻找主角
    /// </summary>
    /// <returns>MapRole角色</returns>
    public static MapRole FindPlayer()
    {
        var obj = GameObject.Find("Level/Player");
        if (obj != null) return obj.GetComponent<MapRole>();
        obj = GameObject.FindWithTag("Player");
        
        if (obj == null)
        {
            //Debug.LogError("找不到主角，请设置层级为Level/Player。或者Tag设置为Player");
            return null;
        }
        return obj.GetComponent<MapRole>();
    }
    
    /// 通过已有数据实例在地图中创建角色
    public static MapRole CreateRoleView(this RoleInstance role, string tag = "NPC")
    {
        var roleViewPre = Jyx2ResourceHelper.GetCachedPrefab("MapRole");
        var roleView = GameObject.Instantiate(roleViewPre).GetComponent<MapRole>();
        role.View = roleView;
        roleView.DataInstance = role;
        roleView.m_RoleKey = role.Id;
        roleView.name = role.Id.ToString();
        roleView.tag = tag;
        return roleView;
    }
    
    /// <summary>
    /// 地图角色绑定新的数据实例
    /// </summary>
    /// <param name="roleKey">角色Key</param>
    /// <param name="roleView">角色模型</param>
    public static void CreateRoleInstance(this MapRole roleView, int roleKey)
    {
        roleView.BindRoleInstance(GameRuntimeData.Instance.AllRoles[roleKey]).Forget();
        roleView.DataInstance.Hp = roleView.DataInstance.MaxHp; //默认满血
    }

    /// <summary>
    /// 地图角色绑定已有数据实例
    /// </summary>
    /// <param name="roleView">角色模型（展示在地图上的）</param>
    /// <param name="role">角色数据/param>
    public static async UniTask BindRoleInstance(this MapRole roleView, RoleInstance role)
    {
        if (role == null || roleView == null)
            return;

        //已经绑定过了，不需要再行绑定了。
        if (role.View == roleView && roleView.DataInstance == role)
            return;
        
        role.View = roleView;
        roleView.DataInstance = role;
    }
}

