using System;
using Cysharp.Threading.Tasks;

namespace Jyx2.Battle
{
    using Jyx2;
    using UnityEngine;
    
    public class BattleUnit : MonoBehaviour
    {
        public RoleInstance  _role ;
        public BattleManager _manager;
        public bool isCd = false;

        public Transform trans;
        public int actPoints = -300; //策略产生 1000 2000

        public int beforeStartTime = 3000;
        //角色是否死亡
        public Boolean isDead  
        {
            set { }
            get  { return _role == null || _role.Hp <= 0; }
        }
        /*public BattleUnit(RoleInstance role)
        {
            _role = role;
        }*/

        async void Start()
        {
            /*GameObject Dialog = Jyx2ResourceHelper.CreatePrefabInstance("Dialog");
            Dialog.SetActive(true);
            Dialog.transform.SetParent(trans);
            //GameObject Dialo g = transform.Find("Dialog").gameObject;
            //Dialog = Object.Instantiate(Dialog);
            Dialog.transform.position = trans.position;*/
            
            /*while (!isDead) //角色死亡行动终止
            {*/
                //await UniTask.Delay(2000);
            
                //if(!_manager.isPause) actPoints = actPoints + _role.Speed;
                
                //计算出行动力够点的按钮 亮灭
                
            //}
            
        }

        async void FixedUpdate()
        {
            //角色死亡则移除脚本
            
            if (_role == null || _role.Hp <= 0 || isCd || _manager.isPause) return;
            _role.Attack = 10; //
            isCd = true;
            if (_manager._player.Id == _role.Id) //主角自选操作
            {
                _manager.operate(_role,this,actPoints);
            }
            else //其他AI攻击
            {
                if (beforeStartTime > 0)
                {
                    await UniTask.Delay(Math.Max(5000 - _role.Speed * 10,0)); // 战斗开始 AI第一次攻击的等待时间
                    beforeStartTime = 0;
                }
                
                _manager.planAndAttack(_role,this,actPoints);
            }
            
        }
        
    }
}