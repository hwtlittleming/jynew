

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Configs;
using i18n.TranslatorDef;
using UnityEngine;

namespace Jyx2
{
    /// 游戏的存档数据结构根节点
    [Serializable]
    public class GameRuntimeData 
    {
        public static GameRuntimeData Instance {
            get
            {
                if(_instance == null)
                {
                    CreateNew();
                }
                return _instance;
            }
        }
        private static GameRuntimeData _instance;
        
        #region 存档数据定义
        //第一个角色为主角，主角携带=背包物品 物品里记录使用人
        [SerializeField] public Dictionary<int,RoleInstance> AllRoles = new Dictionary<int,RoleInstance>();
        
        //当前玩家队伍
        [SerializeField] public List<int> TeamId = new List<int>();
        [SerializeField] public SubMapSaveData SubMapData; //当前所处子地图存储数据
        [SerializeField] public WorldMapSaveData WorldData; //世界地图信息
        
        [SerializeField] public Dictionary<string, string> KeyValues = new Dictionary<string, string>(); //宝箱状态,地图打开状态,天数
        [SerializeField] public Dictionary<string, int> ShopItems= new Dictionary<string, int>(); //小宝商店物品，{ID，数量}
        [SerializeField] public Dictionary<string, int> EventCounter = new Dictionary<string, int>();
        [SerializeField] public Dictionary<string, int> MapPic = new Dictionary<string, int>();
        #endregion
        
        //入口:新游戏的开始
        public static GameRuntimeData CreateNew()
        {
            _instance = new GameRuntimeData();
            
            //创建所有角色
            foreach (var r in GameConfigDatabase.Instance.GetAll<ConfigCharacter>())
            {
                var role = new RoleInstance(r.Id);
                _instance.AllRoles.Add(r.Id, role);
            }
            
            //主角入当前队伍
            _instance.JoinRoleToTeam(0);
            _instance.JoinRoleToTeam(1);

            return _instance;
        }

        #region 游戏保存和读取
        
        public const string ARCHIVE_FILE_NAME = "archive_{0}.dat";
        public const string ARCHIVE_SUMMARY_FILE_NAME = "archive_summary_{0}.dat";
        public const string ARCHIVE_FILE_DIR = "Save";
        
        [Obsolete("待删除")]
        public const string ARCHIVE_SUMMARY_PREFIX = "save_summaryinfo_new_";
        
        //存档
        public void GameSave(int index = -1)
        {
            Debug.Log("存档中.. index = " + index);

            string path = string.Format(ARCHIVE_FILE_NAME, index);
           ES3.Save(nameof(GameRuntimeData), this, path);
           
           /* 序列化方式存储 BinaryFormatter bf0=new BinaryFormatter();
           FileStream  fs0=File.Create(Application.persistentDataPath+"/Data.yj");
           bf0.Serialize(fs0,new SkillInstance());
           //将Save对象转化为字节
           fs0.Close();
           
           BinaryFormatter bf=new BinaryFormatter();
           FileStream fs=File.Open(Application.persistentDataPath+"/Data.yj",FileMode.Open);//打开文件
           SkillInstance save=bf.Deserialize(fs) as SkillInstance;
           fs.Close();*/
           
            Debug.Log("存档结束");

            string mapName = LevelMaster.GetCurrentGameMap().GetShowName();
            var summaryInfo = $"{Player.Level}级,{mapName},队伍:{TeamId.Count}人";

            ES3.Save("summary", summaryInfo, string.Format(ARCHIVE_SUMMARY_FILE_NAME, index));
            //PlayerPrefs.SetString(ARCHIVE_SUMMARY_PREFIX + index, summaryInfo);
        }
        
        //载入存档
        public static bool DoLoadGame(int index)
        {
            //ES3取得存档数据
            string path = string.Format(ARCHIVE_FILE_NAME, index);
            var r =  ES3.Load<GameRuntimeData>(nameof(GameRuntimeData), path);
            if (r == null)return false;
            
            //将存档的角色等各数据给当前runtime实例
            _instance = r;
            for(int ig = 0; ig <_instance.AllRoles.Count; ig++)
            {//读档时，role里的对象数据实例化
                var kv = _instance.AllRoles.ElementAt(ig);
                //configData实例化数据
                kv.Value.configData = GameConfigDatabase.Instance.Get<ConfigCharacter>(kv.Key); 
                //存档中的技能 除了类型为对象的 其他已都有值
                for (int io = 0; io<kv.Value.skills.Count;io++)
                {
                    kv.Value.skills[io].Display = GameConfigDatabase.Instance.Get<SkillDisplayAsset>(kv.Value.skills[io].DisplayId);
                }
            }
            
            //加载地图
            var loadPara = new LevelMaster.LevelLoadPara() {loadType = LevelMaster.LevelLoadPara.LevelLoadType.Load};
            int mapId = -1;
            if (r.SubMapData == null)
            {
                mapId = GameConst.WORLD_MAP_ID;
                loadPara.Pos = r.WorldData.WorldPosition;
                loadPara.Rotate = r.WorldData.WorldRotation;
            }
            else
            {
                mapId = r.SubMapData.MapId;
                loadPara.Pos = r.SubMapData.CurrentPos;
                loadPara.Rotate = r.SubMapData.CurrentOri;
            }

            LevelLoader.LoadGameMap(GameConfigDatabase.Instance.Get<ConfigMap>(mapId), loadPara,
                () => { LevelMaster.Instance.TryBindPlayer(); });
            return true;
        }

        //获取存档时间
        public static DateTime? GetSaveDate(int index)
		{
            var summaryInfoFilePath = string.Format(ARCHIVE_SUMMARY_FILE_NAME, index);
            return ES3.FileExists(summaryInfoFilePath) ?
                 ((DateTime?) ES3.GetTimestamp(summaryInfoFilePath)) : null;
        }

        //获取存档摘要
        public static string GetSaveSummary(int index)
		{
			string summaryInfo = "";
			var summaryInfoFilePath = string.Format(ARCHIVE_SUMMARY_FILE_NAME, index);
			if (ES3.FileExists(summaryInfoFilePath))
			{
				summaryInfo = ES3.Load<string>("summary", summaryInfoFilePath);
			}
            return summaryInfo;
		}
        
        #endregion
        
        #region 游戏运行时数据

        //主角
        public RoleInstance Player
        {
            set { }
            get { return AllRoles[0]; }
        }
        

        //获取队伍所有角色
        public IEnumerable<RoleInstance> GetTeam()
        {
            foreach (var id in TeamId)
            {
                yield return AllRoles[id];
            }
        }
        
        //角色入队
        public bool JoinRoleToTeam(int roleId,bool showGetItem = false)
        {
            if (GetRoleInTeam(roleId) != null)
            {
                Debug.LogError($"错误，角色重复入队：id = {roleId}");
                return false;
            }
            
            var role = AllRoles[roleId];
            if(role == null)
            {
                Debug.LogError($"调用了不存在的role加入队伍，id = {roleId}");
                return false;
            }
            
            //获得角色身上的道具
            if (role.configData.Id != 0)
            {
                foreach (var item in role.Items)
                {
                    if (item.Count == 0) item.Count = 1;
                    Player.AlterItem(item.ConfigId, item.Count,item.Quality);
                    if (item.Count > 0 && showGetItem)
                    {
                        StoryEngine.Instance.DisplayPopInfo("得到物品:".GetContent(nameof(GameRuntimeData)) + item.Name + "×" + Math.Abs(item.Count));
                    }
                }
                //清空角色身上的物品
                role.Items.Clear();
            }

            TeamId.Add(roleId);
            return true;
        }

        public bool LeaveTeam(int roleId) 
        {
            var role = AllRoles[roleId];
            if (role == null)
            {
                Debug.LogError("调用了不存在的role加入队伍，roleid =" + roleId);
                return false;
            }
            if (GetRoleInTeam(roleId) ==null) 
            {
                Debug.LogError("role is not in main team，roleid =" + roleId);
                return false;
            }

            //清除背包中的角色已装备的装备
            foreach (var equipment in role.Equipments)
            {
                if (equipment == null) continue;
                role.Items.Remove(role.GetItem(equipment.Id));
            }
            
            TeamId.Remove(roleId);
            role.Recover(true);
            return true;
        }

        //获取队伍里的角色
        public RoleInstance GetRoleInTeam(int roleId)
        {
            if (TeamId.Contains(roleId))
            {
                return AllRoles[roleId];
            }

            return null;
        }
        
        #region 宝箱状态,地图打开状态

        public string GetKeyValues(string k)
        {
            return KeyValues[k];
        }

        public void SetKeyValues(string k, string v)
        {
            KeyValues[k] = v;
        }

        public void RemoveKey(string k)
        {
            KeyValues.Remove(k);
        }

        public bool KeyExist(string key)
        {
            if (KeyValues.ContainsKey(key) && KeyValues[key] != null)
                return true;
            return false;
        }
        #endregion
        
        //获取背包某物品数量
        public int GetItemCount(String id,int quality = 0)
        {
            return Player.GetItem(id,quality,false).Count;
        }
        
        //设置物品使用人
        public void SetItemUser(String itemId, int roleId)
        {
            Player.GetItem(itemId).UseRoleId = roleId;
        }

        //获取物品使用人
        public int GetItemUser(ItemInstance item)
        {
            return Player.GetItem(item.Id).UseRoleId;
        }
        
        //改变事件
        public void ModifyEvent(int scene, int eventId, int interactiveEventId, int useItemEventId, int enterEventId)
        {
            string key = "evt_" + scene + "_" + eventId;
            KeyValues[key] = string.Format("{0}_{1}_{2}", interactiveEventId, useItemEventId, enterEventId);
        }

        public string GetModifiedEvent(int scene,int eventId)
        {
            string key = "evt_" + scene + "_" + eventId;
            if (KeyValues.ContainsKey(key))
                return KeyValues[key];
            return null;
        }
        
        public void AddEventCount(int scene, int eventId, int eventName, int num)
        {
            string key=(string.Format("{0}_{1}_{2}", scene, eventId, eventName));
            if(EventCounter.ContainsKey(key)){
                EventCounter[key]+=num;
            }else{
                EventCounter[key]=num;
            }
        }
        
        public int GetEventCount(int scene, int eventId, int eventName)
        {
            string key=(string.Format("{0}_{1}_{2}", scene, eventId, eventName));
            if(EventCounter.ContainsKey(key)){
                return EventCounter[key];
            }
            return 0;
        }


        public void SetMapPic(int scene, int eventId, int pic)
        {
            string key=(string.Format("{0}_{1}", scene, eventId));
            if(MapPic.ContainsKey(key) && pic==-1){
                MapPic.Remove(key);
            }else{
                MapPic[key]=pic;
            }
        }
        
        public int GetMapPic(int scene, int eventId)
        {
            string key=(string.Format("{0}_{1}", scene, eventId));
            if(MapPic.ContainsKey(key)){
                return MapPic[key];
            }
            return -1;
        }

        //JYX2场景相关记录
        public Dictionary<string,string> GetSceneInfo(string scene)
        {
            string key = "scene_" + scene;
            if (KeyValues.ContainsKey(key))
            {
                string str = KeyValues[key];
                var rst = ES3.Deserialize<Dictionary<string, string>>(Encoding.UTF8.GetBytes(str));
                return rst;
            }
                
            return null;
        }

        public void SetSceneInfo(string scene, Dictionary<string, string> info)
        {
            if (info == null)
                return;
            string key = "scene_" + scene;

            var str = Encoding.UTF8.GetString(ES3.Serialize(info));
            KeyValues[key] = str;
        }

        /// 获取场景进入条件码
        public int GetSceneEntranceCondition(int mapId)
        {
            var gameMap = ConfigMap.Get(mapId);
            if (gameMap == null) return -1;
            
            //大地图
            if (gameMap.IsWorldMap())
                return 0;

            //已经有地图打开的纪录
            string key = "SceneEntraceCondition_" + gameMap.Id;
            if (KeyValues.ContainsKey(key))
            {
                return int.Parse(GetKeyValues(key));
            }

            //否则取配置表初始值
            return gameMap.EnterCondition;
        }

        /// <summary>
        /// 设置场景进入条件码
        /// </summary>
        public void SetSceneEntraceCondition(int mapId,int value)
        {
            string key = "SceneEntraceCondition_" + mapId;
            SetKeyValues(key, value.ToString());
        }
        #endregion

        private DateTime _startDate;
        public DateTime startDate{
            get {
                if (_startDate.Year == 1)
                {
                    _startDate = DateTime.Now;
                }
                return _startDate;
            }
            set { _startDate = value; }
        }
    }
}
