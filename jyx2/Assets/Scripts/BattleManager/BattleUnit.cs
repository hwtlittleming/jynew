namespace Jyx2.Battle
{
    using Jyx2;
    using UnityEngine;
    
    public class BattleUnit : MonoBehaviour
    {
        public RoleInstance  _role ;
        public BattleManager _manager;
        public bool isActing = false;

        /*public BattleUnit(RoleInstance role)
        {
            _role = role;
        }*/

        async void Start()
        {
        }

        async void FixedUpdate()
        {
            if (_role == null || _role.Hp <= 0 || isActing == true) return;
            _role.Attack = 1;
            isActing = true;
            if (_role.Id == 0 )
            {
                //isActing = true;
                _manager.operate(_role);
            }
            else
            {
                _manager.planAndAttack(_role,this);
            }

            /*await UniTask.Delay(2000);
            isActing = false;*/
            /*Debug.Log("dd");
            await UniTask.Delay(2000);
            isActing = false;
            Debug.Log(_role.Name);*/
            
        }

        public void test()
        {
            Debug.Log("ssssssssssssssssssssssssssssssssssssss");
        }

    }
}