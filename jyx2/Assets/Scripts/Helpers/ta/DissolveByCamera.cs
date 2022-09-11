
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Random = UnityEngine.Random;

namespace Jyx2
{
    public class DissolveByCamera : MonoBehaviour
    {
        private GameObject Player;

        [HideInInspector]
        private Material[] Mats;

        void Start()
        {
        }

        void FixedUpdate()
        {
            TrySetPlayer();
        }

        void TrySetPlayer()
        {
            if(Player == null)
            {
                var p = RoleHelper.FindPlayer();
                if (p == null) return;
                Player = p.gameObject;
            }

            Shader.SetGlobalVector("_PlayerPos", Player.transform.position);
        }

    }
}
