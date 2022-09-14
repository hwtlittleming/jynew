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

        public int actPoints = -300; //策略产生 1000 2000
        
        public Boolean isPause = false; //是否暂停 策略中会暂停

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
            while (!isDead) //角色死亡行动终止
            {
                await UniTask.Delay(1000);
                if(!isPause) actPoints = actPoints + _role.Speed;
               
                //计算出行动力够点的按钮 亮灭
                
            }
            
        }

        async void FixedUpdate()
        {
            //角色死亡则移除脚本
            
            if (_role == null || _role.Hp <= 0 || isCd || isPause) return;
            _role.Attack = 1;
            isCd = true;
            if (_manager._player.Id == _role.Id) //主角自选操作
            {
                _manager.operate(_role,this,actPoints);
            }
            else //其他AI攻击
            {
                if (beforeStartTime > 0)
                {
                    await UniTask.Delay(Math.Max(3000 - _role.Speed * 10,0)); // 战斗开始 AI第一次攻击的等待时间
                    beforeStartTime = 0;
                }
                
                _manager.planAndAttack(_role,this,actPoints);
            }
            
        }
        
    }
}