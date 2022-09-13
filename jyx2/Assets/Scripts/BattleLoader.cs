
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    
    // Start is called before the first frame update
    async void Start()
    {
        await BeforeSceneLoad.loadFinishTask;
        await LoadJyx2Battle(m_BattleId, Callback);
    }
    
    GameRuntimeData runtime
    {
        get { return GameRuntimeData.Instance; }
    }

    //传入 特征值:概率的数组 按传入概率求随机特征值的公用方法
    public int MyRandom(List<SampleRate> lr,Random seed)
    {
        int ran = seed.Next(1,101); 
        int minRate = 0;
        int result = int.Parse(lr.FirstOrDefault().Sample);
        foreach (SampleRate s in lr)
        {
            minRate += s.Rate;
            if (ran <= minRate)
            {   
                //Debug.Log(ran + "-----" + result);
                result = int.Parse(s.Sample);
                return result;
            }
        }
        Debug.Log("概率和不为100");
        return result;
    }

    //对象深拷贝方法
    public static T DeepCopy<T>(T obj)
    {
        //如果是字符串或值类型则直接返回
        if (obj is string || obj.GetType().IsValueType) return obj;
 
        object retval = Activator.CreateInstance(obj.GetType());
        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        foreach (FieldInfo field in fields)
        {
            try { field.SetValue(retval, DeepCopy(field.GetValue(obj))); }
            catch { }
        }
        return (T)retval;
    }
    
    async UniTask LoadJyx2Battle(int id, Action<BattleResult> callback)
    {   
        Debug.Log("-----------BattleLoader.LoadJyx2Battle");
        //todo 上中下三排5*3半透明格子，宠物中(最多三个，三种站位),在第一排才能普攻，普攻是锁定到人的，魔法可以躲，人物在中下，可控制移动到邻近一格，消耗行动力，行动力用完，行动按钮组置灰，行动力决定于一口气的长度 
        //下能打上中下(后期地图变化红黄蓝格代替)，有弓等才能配置在中下，上:普攻只能打中上，魔法不限制
        //攻击先点击选择攻击格子，再按普攻和魔法，则会以选中点为中心实时攻击，魔法会有延迟；未因什么而阻断时怪物和帮手都是按行动力即时开始攻击
        //被打对象无受击动画，额外施加受击掉血效果

        //单例的从存档读取的GameRuntimeData
        if (GameRuntimeData.Instance == null) //如果直接从场景进入战斗，读取配置的初始数据
        {
            GameRuntimeData.CreateNew();
        }
        //需要传入地图或线路  暂时搞个id对应地图的对应关系，以后改成直接传入地图或线路名
        //todo 战场仅几种通用类型，特殊再设计，模糊化当前场景，再实化战斗要素
        //todo 改成从excel取数据
        ConfigBattle battle = ConfigBattle.Get(id);
        if (battle == null)
        {
            Debug.LogError("载入了未定义的战斗，id=" + id);
            return;
        }
        AudioManager.PlayMusic(battle.Music);
        
        //敌方角色生成
        List<SampleRate> lr = battle.RoleRate;
        List<int> enermyIdList = new List<int>();
        List<RoleInstance> enermyRoleList = new List<RoleInstance>(); //战时对方角色 改到类属性里
        List<RoleInstance> ourRoleList = new List<RoleInstance>();//战时我方角色
        if (battle.BattleKind == "0")  //随机遇怪
        {
            //按配置生成数量
            String[] CountLevel = battle.CountLevel.Split('-');
            int count =  UnityEngine.Random.Range(int.Parse(CountLevel[0]),int.Parse(CountLevel[1]));
            
            //求取每个数量生成的怪物
            lr.Sort();
            Random seed = new Random();
            for (int i = 1; i <= count; i++)
            {
                int result = MyRandom(lr,seed);
                enermyIdList.Add(result);
            }
        }else if (battle.BattleKind == "1")  //固定战斗 SampleRate配置角色id就行
        {
            foreach (SampleRate sampleRate in lr)
            {
                enermyIdList.Add(int.Parse(sampleRate.Sample));
            }
        }
        foreach (int roleId in enermyIdList)
        {
            RoleInstance r = new RoleInstance();
            RoleInstance s = runtime.AllRoles[roleId];
            r = DeepCopy(s);
            r.skills = s.skills;
            r.configData.Model = s.configData.Model;//这些gameobject深拷贝不进去 赋值引用
            r.Zhaoshis = s.Zhaoshis;
            r.Items = s.Items;
            r.team = 1;
            enermyRoleList.Add(r);
        }

        //我方角色id生成，如果地图配置了上阵角色
        if (battle.AutoTeamMates.Count > 0)
        {
            foreach (var v in battle.AutoTeamMates)
            {
                var roleId = v.Id;
                if (roleId == -1) continue;
                ourRoleList.Add(runtime.AllRoles[roleId]);
            }
        }
        else //否则我方上阵参战的角色
        {
            List<RoleInstance> teamRole = runtime.GetTeam().ToList();
            foreach (RoleInstance role in teamRole)
            {
                if (role.isReadyToBattle)//选择了参战
                {
                    ourRoleList.Add(role);
                }
            }
        }
        //初始化格子
        BattleboxHelper.Instance.initBattleBlockData();

        //两队角色落地图上的点,主角落在标签为Player的格子
        Transform pos = GameObject.FindWithTag("Player").transform;
        Vector3 position = pos.position;
        int x = int.Parse(pos.name.Split('-')[1]);
        int y = int.Parse(pos.name.Split('-')[2]);
        for (int i = 0; i < ourRoleList.Count; i++)
        {
            RoleInstance r = ourRoleList[i];
            if (r.Id == 0)
            {
                x = int.Parse(pos.name.Split('-')[1]);
                y = int.Parse(pos.name.Split('-')[2]);
                //r.Block = new Vector3(x,y,r.team);
                BattleBlockData bd = BattleManager.Instance.GetBlockData(x,y,"we" );//给角色信息里 添加位置
                position = bd.WorldPos;
                r.blockData = bd;
                bd.role = r; //格子信息里也添加角色
            }
            else // 根据攻击距离加载队友位置 暂时按顺序加载
            {
                int dis = r.bestAttackDistance;
                x = i+1; //UnityEngine.Random.Range(1, BattleManager.Instance.block_list.FirstOrDefault().maxX);
                y = dis;
                //r.Block = new Vector3(x, y, r.team);
                BattleBlockData bd = BattleManager.Instance.GetBlockData(x,dis,"we" );//给角色信息里 添加位置
                position = bd.WorldPos;
                r.blockData = bd;
                bd.role = r; //格子信息里也添加角色
            }
            await CreateRole(r, 0, position);//在BattleRoles下创建角色的模型
        }
        //敌人的位置加载
        for (int i = 0; i < enermyRoleList.Count; i++)
        {
            RoleInstance r = enermyRoleList[i];
            int dis = r.bestAttackDistance;
            x = i+1;//UnityEngine.Random.Range(1, BattleManager.Instance.block_list.FirstOrDefault().maxX);
            y = dis;
            //r.Block = new Vector3(x, y, r.team);
            BattleBlockData bd = BattleManager.Instance.GetBlockData(x,dis,"they" );//给角色信息里 添加位置
            position = bd.WorldPos;
            r.blockData = bd;
            bd.role = r; //格子信息里也添加角色
            await CreateRole(r, 1, position);//在BattleRoles下创建角色的模型
        }
        await BattleManager.Instance.StartBattle(enermyRoleList,ourRoleList,callback);
    }

    UniTask CreateRole(RoleInstance role, int team, Vector3 pos)
    {
        GameObject npcRoot = GameObject.Find("BattleRoles");
        if (npcRoot == null)
        {
            npcRoot = new GameObject("BattleRoles");
        }

        MapRole roleView = role.CreateRoleView();

        roleView.IsInBattle = true;
        
        roleView.transform.SetParent(npcRoot.transform, false);
        roleView.transform.position = pos;

        role.team = team;
        return roleView.RefreshModel(); //刷新模型
    }
}
