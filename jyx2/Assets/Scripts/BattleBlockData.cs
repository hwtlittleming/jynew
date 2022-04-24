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

namespace Jyx2
{
    public class BattleBlockData
    {
        //战场逻辑位置 待去掉
        public BattleBlockVector BattlePos;

        //实际对应的世界坐标系的点
        public Vector3 WorldPos; 

        //格子的队伍 we they
        public String team;
        
        //当前所处格子编号
        public int x;
        public int y;

        //格子名称
        public String blockName;
        
        //格子上的角色
        public RoleInstance role;
        
        //格子的游戏对象
        public GameObject blockObject;
        
        //对应绘制的对象
        public GameObject gameObject;

        public int maxX = 1;

        public int maxY = 1;

        public bool IsActive
        {
            get { return _isActive; }
        }
        private bool _isActive = false;

        public bool Inaccessible { get; internal set; }

        public void Show()
        {
            gameObject.layer = 0;
            foreach(Transform go in gameObject.transform)
            {
                go.gameObject.layer = 0;
            }
            _isActive = true;
        }

        public void Hide()
        {
            gameObject.layer = 17;
            foreach (Transform go in gameObject.transform)
            {
                go.gameObject.layer = 17;
            }
            _isActive = false;
        }
    }
}
