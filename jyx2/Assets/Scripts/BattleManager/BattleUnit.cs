namespace Jyx2.Battle
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Cinemachine;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using i18n.TranslatorDef;
    using Jyx2;

    using Jyx2.Battle;
    using Jyx2.Middleware;
    using Jyx2Configs;
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
            if (_role == null || isActing == true) return;
            _role.Attack = 1;
            isActing = true;

            _manager.planAndAttack(_role);

            Debug.Log("dd");
            await UniTask.Delay(2000);
            isActing = false;
            Debug.Log(_role.Name);
            
        }
        
    }
}