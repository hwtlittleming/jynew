
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Configs;
using Cysharp.Threading.Tasks;
using Jyx2;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Random = System.Random;

/// 战斗启动器
public class BattleLoader : MonoBehaviour
{
    [LabelText("载入战斗ID")] public int m_BattleId = 0;
    [HideInInspector] public Action<BattleResult> Callback;
    List<int> publicPosition = new List<int>() {3,2,4}; //公共格位置顺序;
    List<int> defaultPosition = new List<int>() {3,1,5,2,4}; //该行主角已占一位
    List<int> defaultPosition2 = new List<int>() {3,1,5,2,4}; // 一排5格子的地图的默认战位
    List<int> defaultPosition3 = new List<int>() {3,1,5,2,4}; 
    List<int> defaultPosition4 = new List<int>() {3,1,5,2,4}; 
    // Start is called before the first frame update
    async void Start()
    {
        await BeforeSceneLoad.loadFinishTask;
        
        //获得战斗全局配置  随机遇怪时 遇怪的数量级和概率
        Dictionary<String,String> allEnermyConfig =  GameConst.mapEnermy;

        //测试给默认值
        int battleMapId = 1;
        String configId = "1";
        allEnermyConfig.TryGetValue(configId, out String ranEnermy);
        
        
        // 1)战斗地图id 2)战斗类型:0随机1固定2混合  3)随机时，生成概率 4)我方限制队友 5)我方额外队友 6)地方固定角色 7)回调函数
        await LoadBattle(battleMapId,"0",ranEnermy,null,null,null,Callback);
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
    
    //带传参数     
    async UniTask LoadBattle(int battleMapId,String battleKind,String ranEnermy,List<String> limitOurRole,List<String> ExtraOurRole,List<String> fixedEnermy,Action<BattleResult> callback)
    {   
        Debug.Log("-----------BattleLoading...");
        //上中下三排5*3半透明格子，普攻锁定到人，魔法可以躲 攻击距离拳头1刀剑枪2 弓杖3
        
        //单例的从存档读取的GameRuntimeData
        if (GameRuntimeData.Instance == null) //如果直接从场景进入战斗，读取配置的初始数据
        {
            GameRuntimeData.CreateNew();
        }

        #region 加载地图数据，战斗双方数据
        #endregion
        
        //若传入指定战斗地图id，则取，未传或取不到，则取当前地图对应的战斗地图
        ConfigBattle battle = ConfigBattle.Get(battleMapId);
        if (battleMapId == -1 || battle == null)
        {
           ConfigMap map = LevelMaster.GetCurrentGameMap();
           battle = ConfigBattle.Get(int.Parse(map.BattleMapKind)) ;
        }
       // AudioManager.PlayMusic(battle.Music);  音乐
        
        
        //我方战斗角色 格子位置 + roleInstance
       List<int> ourRoleList_temp =new List<int>();
        Dictionary<String,RoleInstance> ourRoleDic = new Dictionary<String,RoleInstance>{};
        List<RoleInstance> tr = runtime.GetTeam().ToList().Where(r => (r.isInBattle = true)).ToList(); //我方参战角色
        if (limitOurRole != null)
        {
            foreach (var v in limitOurRole)
            {
                ourRoleList_temp.Add(int.Parse(v));
            }
        }
        else
        {
            foreach (var v in tr)
            {
                ourRoleList_temp.Add(v.Id);
            }
        }
        if (ExtraOurRole != null)
        {
            foreach (var v2 in ExtraOurRole)
            {
                ourRoleList_temp.Add(int.Parse(v2));
            }
        }
        tr = copyRole(ourRoleList_temp);
        
        //初始化格子
        BattleboxHelper.Instance.initBattleBlockData();
        
        //我方角色落点,主角落在标签为Player的格子
        Transform pos = GameObject.FindWithTag("Player").transform;
        Vector3 position = pos.position;
        int x = int.Parse(pos.name.Split('-')[1]);
        int y = int.Parse(pos.name.Split('-')[2]);
        for (int i = 0; i < tr.Count; i++)
        {
            RoleInstance r = tr[i];
            BattleBlockData bd = null;
            if  ( i == 0 )  //认第一个放进去的为主角
            {
                bd = BattleManager.Instance.GetBlockData(x,y,"we" );
                position = bd.WorldPos;
                r.blockData = bd;//给角色信息里 添加位置
                defaultPosition.Remove(x);
            }
            else // 根据攻击距离加载队友位置 
            {
                int dis = r.bestAttackDistance;
                if (dis == 1)
                {
                    if (!publicPosition.IsNullOrEmpty())
                    {
                        x = publicPosition[0];
                        publicPosition.Remove(x);
                    }
                    else
                    {
                        dis = 2;
                    }
                }
                if (dis == 2)
                {
                    if (!defaultPosition.IsNullOrEmpty())
                    {
                        x = defaultPosition[0];
                        defaultPosition.Remove(x);
                    }
                    else
                    {
                        dis = 3;
                    }
                }
                if (dis == 3)
                {
                    if (!defaultPosition2.IsNullOrEmpty())
                    {
                        x = defaultPosition2[0];
                        defaultPosition2.Remove(x);
                    }
                    else
                    {
                        continue;
                    }
                }
                
                bd = BattleManager.Instance.GetBlockData(x,dis - 1,"we" );//给角色信息里 添加位置
                position = bd.WorldPos;
                r.blockData = bd;
            }
            ourRoleDic.Add(bd.blockName,r);
            await CreateRole(r, 0, position);//在BattleRoles下创建角色的模型
        }
        
        //敌方角色生成
        String countRan = ranEnermy.Split(";")[0];
        String[] enermyRan = ranEnermy.Split(";")[1].Split(",");
        List<SampleRate> lr = new List<SampleRate>();
        foreach (var ran in enermyRan) //后可调配置方法更直观
        {
            SampleRate s = new SampleRate();
            s.Sample = ran.Split(":")[0];
            s.Rate = int.Parse(ran.Split(":")[1]);
            lr.Add(s);
        }
        List<int> enermyIdList = new List<int>();
        List<RoleInstance> enermyRoleList = new List<RoleInstance>(); //战时对方角色 改到类属性里
        Dictionary<String,RoleInstance> enermyRoleDic = new Dictionary<String,RoleInstance>{};
        if (battleKind == "0")  //随机遇怪
        {
            //按配置生成数量
            String[] CountLevel = countRan.Split('-');
            int count =  UnityEngine.Random.Range(int.Parse(CountLevel[0]),int.Parse(CountLevel[1]) + 1);
            
            //求取每个数量生成的怪物
            lr.Sort();
            Random seed = new Random();
            for (int i = 1; i <= count; i++)
            {
                int result = MyRandom(lr,seed);
                enermyIdList.Add(result);
            }
        }else if (battleKind == "1")  //固定配置战斗
        {
            foreach (String s in fixedEnermy)
            {
                enermyIdList.Add(int.Parse(s));
            }
        }
        enermyRoleList =  copyRole(enermyIdList,1);
        //敌人的位置加载
        for (int i = 0; i < enermyRoleList.Count; i++)
        {
            RoleInstance r = enermyRoleList[i];
            int dis = r.bestAttackDistance;
            if (dis == 1)
            {
                if (!publicPosition.IsNullOrEmpty())
                {
                    x = publicPosition[0];
                    publicPosition.Remove(x);
                }
                else
                {
                    dis = 2;
                }
            }
            if (dis == 2)
            {
                if (!defaultPosition3.IsNullOrEmpty())
                {
                    x = defaultPosition3[0];
                    defaultPosition3.Remove(x);
                }
                else
                {
                    dis = 3;
                }
            }
            if (dis == 3)
            {
                if (!defaultPosition4.IsNullOrEmpty())
                {
                    x = defaultPosition4[0];
                    defaultPosition4.Remove(x);
                }
                else
                {
                    continue;
                }
            }
            BattleBlockData bd = BattleManager.Instance.GetBlockData(x,dis - 1,"they" );
            position = bd.WorldPos;
            r.blockData = bd;//给角色信息里 添加位置
            enermyRoleDic.Add(bd.blockName,r);
            await CreateRole(r, 1, position);//在BattleRoles下创建角色的模型
        }
        await BattleManager.Instance.StartBattle(enermyRoleDic,ourRoleDic,callback);
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

    //给战斗对象深拷贝
    public List<RoleInstance> copyRole(List<int> rList,int team = 0)
    {
        List<RoleInstance> result = new List<RoleInstance>() ;
        foreach (int roleId in rList)
        {
            RoleInstance r = new RoleInstance();
            RoleInstance s = runtime.AllRoles[roleId];
            r = DeepCopy(s);
            r.skills = s.skills;
            r.configData = s.configData;//这些gameobject深拷贝不进去 赋值引用
            r.Items = s.Items; //物品引用的对象仍是相同的
            r.team = team;
            result.Add(r);
        }
        return result;
    }
}
