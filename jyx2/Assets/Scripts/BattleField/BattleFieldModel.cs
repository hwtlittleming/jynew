
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jyx2
{
    public enum BattleResult
    {
        Win,
        Lose,
        InProgress,
    }
    
    public class BattleFieldModel
    {
        //行动集气
        const float ActionSp = 1000f;

        //参与战斗的角色
        public List<RoleInstance> Roles = new List<RoleInstance>();

        //死亡的角色
        public List<RoleInstance> Dead = new List<RoleInstance>();

        public List<RoleInstance> AliveRoles
        {
            get
            {
                var roleList = Roles.FindAll(role => !role.IsDead());
                roleList.Sort();
                return roleList;
            }
        }

        //队友
        public List<RoleInstance> Teammates
        {
            get
            {
                return Roles.FindAll((role) => role.team == 0);
            }
        }

        //敌人
        public List<RoleInstance> Enemys
        {
            get
            {
                return Roles.FindAll((role) => role.team > 0);
            }
        }

        //战斗结果回调
        public Action<BattleResult> Callback;
        
        //增加一个战斗角色
        public void AddBattleRole(RoleInstance role, BattleBlockVector pos, int team, bool isAI)
        {
            role.BattleModel = this;
            role.Pos = pos;
            role.team = team;
            role.isActed = false;
            role.isWaiting = false;
            if (!Roles.Contains(role)) Roles.Add(role);
        }

        public bool BlockHasRole(int x, int y)
        {
            return BlockRoleTeam(x, y) != -1;
        }

        public int BlockRoleTeam(int x, int y)
        {
            var role = GetAliveRole(new Vector3(x, y));
            if (role != null) return role.team;
            return -1;
        }

        public RoleInstance GetAliveRole(Vector3 vec)
        {
            foreach(var r in Roles)
            {
                if (r.IsDead()) continue;
                /*if(r.Block.Equals(vec))
                {
                    return r;
                }*/
            }
            return null;
        }

        //战斗是否结束
        public BattleResult GetBattleResult()
        {
            Dictionary<int, int> teamCount = new Dictionary<int, int>();
            foreach(var role in Roles)
            {
                if (role.IsDead()) continue;
                
                if(!teamCount.ContainsKey(role.team))
                    teamCount.Add(role.team, 0);

                teamCount[role.team]++;
            }

            //战斗进行中
            if (teamCount.Keys.Count > 1)
                return BattleResult.InProgress;

            //我方有角色，胜利
            if (teamCount.ContainsKey(0))
                return BattleResult.Win;

            //敌方有角色，失败
            return BattleResult.Lose;
        }
        
    }
}
