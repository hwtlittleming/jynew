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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using i18n.TranslatorDef;
using Jyx2;

using Jyx2;
using Jyx2Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

/// <summary>
/// 战斗启动器
/// </summary>
public class BattleLoader : MonoBehaviour
{
    [LabelText("载入战斗ID")] public int m_BattleId = 0;


    [HideInInspector] public Action<BattleResult> Callback;


    public bool IsTestCase = false;
    bool setPlayer = false;
    
    public struct BattlePosRole
    {
        public string pos;

        public int team;

        public int roleKey;
    }

    public List<BattlePosRole> m_Roles;

    void CycleLoadBattle()
    {
        LevelLoader.LoadBattle(m_BattleId, (ret) => { CycleLoadBattle(); });
    }

    // Start is called before the first frame update
    async void Start()
    {
        await BeforeSceneLoad.loadFinishTask;

        if (IsTestCase)
        {
            await LoadJyx2Battle(m_BattleId, (ret) => { CycleLoadBattle(); });
        }
        else
        {
            await LoadJyx2Battle(m_BattleId, Callback);
        }
    }


    GameRuntimeData runtime
    {
        get { return GameRuntimeData.Instance; }
    }

    //传入 特征值:概率的数组 按传入概率求随机特征值的公用方法
    public int MyRandom(List<SamepleRate> lr,Random seed)
    {
        int ran = seed.Next(1,101); 
        int minRate = 0;
        int result = int.Parse(lr.FirstOrDefault().Sameple);
        foreach (SamepleRate s in lr)
        {
            minRate += s.Rate;
            if (ran <= minRate)
            {   
                //Debug.Log(ran + "-----" + result);
                result = int.Parse(s.Sameple);
                return result;
            }
        }
        
        Debug.Log("概率和不为100");
        return result;
    }

    async UniTask LoadJyx2Battle(int id, Action<BattleResult> callback)
    {   //todo 得改成预先配置的参战角色，免得每次战斗都要选麻烦 
        Debug.Log("-----------BattleLoader.LoadJyx2Battle");
        //todo 上中下三排5*3半透明格子，宠物中(最多三个，三种站位),在第一排才能普攻，普攻是锁定到人的，魔法可以躲，人物在中下，可控制移动到邻近一格，消耗行动力，行动力用完，行动按钮组置灰，行动力决定于一口气的长度 
        //下能打上中下(后期地图变化红黄蓝格代替)，有弓等才能配置在中下，上:普攻只能打中上，魔法不限制
        //攻击先点击选择攻击格子，再按普攻和魔法，则会以选中点为中心实时攻击，魔法会有延迟；未因什么而阻断时怪物和帮手都是按行动力即时开始攻击
        //白色格子可到，黑色不可到
        //被打对象无受击动画，额外施加受击掉血效果
        // 1.画战场 2.加载战斗单位 3.加载战斗UI 4.战斗进行 5.战斗结束，根据结果增改数据
        // todo 在战斗场景中预设 4*5每个格子一个节点位置，若是随机遇怪或是固定战斗，都从battleData读取战斗信息，生成怪物，分配位置；
        
        
        //单例的从存档读取的GameRuntimeData
        if (GameRuntimeData.Instance == null)
        {
            GameRuntimeData.CreateNew();
        }
        //需要传入地图或线路  暂时搞个id对应地图的对应关系，以后改成直接传入地图或线路名
        m_Roles = new List<BattlePosRole>();
        //todo 战场仅几种通用类型，特殊再设计，模糊化当前场景，再实化战斗要素
        Jyx2ConfigBattle battle = Jyx2ConfigBattle.Get(id);
        if (battle == null)
        {
            Debug.LogError("载入了未定义的战斗，id=" + id);
            return;
        }
        //敌方角色生成
        List<SamepleRate> lr = battle.RoleRate;
        List<int> enermyIdList = new List<int>();
        if (battle.BattleKind == "0")  //随机遇怪
        {
            //生成数量
            String[] CountLevel = battle.CountLevel.Split('-');
            int count = new Random().Next(int.Parse(CountLevel[0]), int.Parse(CountLevel[1]) + 1);
            
            //求取每个数量生成的怪物
            lr.Sort();
            Random seed = new Random();
            for (int i = 1; i <= count; i++)
            {
                int result = MyRandom(lr,seed);
                enermyIdList.Add(result);
            }
        }else if (battle.BattleKind == "1")  //固定战斗
        {
            foreach (SamepleRate samepleRate in lr)
            {
                enermyIdList.Add(int.Parse(samepleRate.Sameple));
            }
        }
        List<RoleInstance> enermyRoleList = new List<RoleInstance>();
        foreach (int roleId in enermyIdList)
        {
            enermyRoleList.Add(runtime.GetRole(roleId));
        }
        
        AudioManager.PlayMusic(battle.Music);
        
        //我方角色生成，如果地图配置了上阵角色
        List<RoleInstance> ourRoleList = new List<RoleInstance>();
        if (battle.AutoTeamMates.Count > 0)
        {
            foreach (var v in battle.AutoTeamMates)
            {
                var roleId = v.Id;
                if (roleId == -1) continue;
                ourRoleList.Add(runtime.GetRole(roleId));
            }
        }
        else //否则我方上阵参战的角色
        {
            List<RoleInstance> teamRole = runtime.GetTeam().ToList();
            foreach (RoleInstance role in teamRole)
            {
                if (true) // todo 加字段：选择了参战的
                {
                    ourRoleList.Add(role);
                }
            }
        }
        //两队角色落地图上的点,主角落在标签为Player的格子
        Transform pos = GameObject.FindWithTag("Player").transform;
        foreach (RoleInstance r in ourRoleList)
        {
            if (r.GetJyx2RoleId() != 0)
            {
                // todo 根据角色种类加载到不同的前 中 后排位置 
            }
            r.Block = new Vector3(int.Parse(pos.name.Split('-')[1]),int.Parse(pos.name.Split('-')[2]),r.team);
            r.blockData.x = int.Parse(pos.name.Split('-')[1]);
            r.blockData.y = int.Parse(pos.name.Split('-')[2]);
            r.blockData.blockName = pos.name;
            r.ExpGot = 0;
            await CreateRole(r, 0, pos);//在BattleRoles下创建角色的模型
        }
        Transform epos = GameObject.Find("block_parent/they-1-2").transform;
        Transform epos1 = GameObject.Find("block_parent/they-1-2").transform;
        Transform epos2 = GameObject.Find("block_parent/they-2-2").transform;
        Transform epos3 = GameObject.Find("block_parent/they-3-2").transform;
        Transform epos4 = GameObject.Find("block_parent/they-4-2").transform;
        Transform epos5 = GameObject.Find("block_parent/they-5-2").transform;
        List<Transform> t = new List<Transform>();
        t.Add(epos1);t.Add(epos2);t.Add(epos3);t.Add(epos4);t.Add(epos5);
        int ii = 0;
        // todo 敌人的位置加载
        foreach (RoleInstance r in enermyRoleList)
        {
            epos = t[ii];
            r.Block = new Vector3(int.Parse(epos.name.Split('-')[1]),int.Parse(epos.name.Split('-')[2]),r.team);
            r.blockData.blockName = epos.name;
            r.ExpGot = 0;
            await CreateRole(r, 1, epos);//在BattleRoles下创建角色的模型
            ii++;
        }
        await BattleManager.Instance.StartBattle(enermyRoleList,ourRoleList,callback);
    }

    UniTask CreateRole(RoleInstance role, int team, Transform pos)
    {
        //Debug.Log($"--------BattleLoader.CreateRole, role={role.Name}, team={team}, pos={pos.name}");
        role.LeaveBattle();
        //find or create
        GameObject npcRoot = GameObject.Find("BattleRoles");
        if (npcRoot == null)
        {
            npcRoot = new GameObject("BattleRoles");
        }

        MapRole roleView;
        //设置主角
        if (!setPlayer && role.GetJyx2RoleId() == 0)
        {
            setPlayer = true;
            roleView = role.CreateRoleView("Player");
        }
        roleView = role.CreateRoleView();

        roleView.IsInBattle = true;
        
        roleView.transform.SetParent(npcRoot.transform, false);
        roleView.transform.position = pos.position;

        role.team = team;
        return roleView.RefreshModel(); //刷新模型
    }
}
