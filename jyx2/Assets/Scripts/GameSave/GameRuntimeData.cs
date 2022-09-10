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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using ES3Types;
using i18n.TranslatorDef;
using Jyx2Configs;
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
        //JYX2，所有的角色都放到存档里
        [SerializeField] public Dictionary<int,RoleInstance> AllRoles = new Dictionary<int,RoleInstance>();
        
        //当前玩家队伍
        [SerializeField] private List<int> TeamId = new List<int>();
        [SerializeField] public SubMapSaveData SubMapData; //当前所处子地图存储数据
        [SerializeField] public WorldMapSaveData WorldData; //世界地图信息
        
        [SerializeField] public Dictionary<string, string> KeyValues = new Dictionary<string, string>(); //主键值对数据
        [SerializeField] public Dictionary<string, int> Items = new Dictionary<string, int>(); //JYX2物品，{ID，数量}
        [SerializeField] public Dictionary<string, int> ItemUser= new Dictionary<string, int>(); //物品使用人，{物品ID，人物ID}
        [SerializeField] public Dictionary<string, int> ShopItems= new Dictionary<string, int>(); //小宝商店物品，{ID，数量}
        [SerializeField] public Dictionary<string, int> EventCounter = new Dictionary<string, int>();
        [SerializeField] public Dictionary<string, int> MapPic = new Dictionary<string, int>();
        [SerializeField] private List<int> ItemAdded = new List<int>(); //已经添加的角色物品
        #endregion
        
        //入口:新游戏的开始
        public static GameRuntimeData CreateNew()
        {
            _instance = new GameRuntimeData();
            
            //创建所有角色
            foreach (var r in GameConfigDatabase.Instance.GetAll<Jyx2ConfigCharacter>())
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

        public static string GetJson<T>(T obj)
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                json.WriteObject(ms, obj);
                string szJson = Encoding.UTF8.GetString(ms.ToArray());
                return szJson;
            }
        }
        
        public static T ParseFormJson<T>(string szJson)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                DataContractJsonSerializer dcj = new DataContractJsonSerializer(typeof(T));
                return (T)dcj.ReadObject(ms);
            }
        }
        
        //存档
        public void GameSave(int index = -1)
        {
            Debug.Log("存档中.. index = " + index);

            string path = string.Format(ARCHIVE_FILE_NAME, index);
           //ES3.Save(nameof(GameRuntimeData), this, path);
           
           /*ES3.Save(nameof(GameRuntimeData), GetJson(this) , path);
           GameRuntimeData g = ParseFormJson<GameRuntimeData>(ES3.Load(path).ToString());*/

           ES3.Save(nameof(GameRuntimeData), new SkillInstance(), path);
           
           /*BinaryFormatter bf0=new BinaryFormatter();
           FileStream  fs0=File.Create(Application.persistentDataPath+"/Data.yj");
           bf0.Serialize(fs0,new SkillInstance());
           //将Save对象转化为字节
           fs0.Close();
           
           BinaryFormatter bf=new BinaryFormatter();
           FileStream fs=File.Open(Application.persistentDataPath+"/Data.yj",FileMode.Open);//打开文件
           SkillInstance save=bf.Deserialize(fs) as SkillInstance;
           fs.Close();*/
           
            Debug.Log("存档结束");

            var summaryInfo = GenerateSaveSummaryInfo();
            ES3.Save("summary", summaryInfo, string.Format(ARCHIVE_SUMMARY_FILE_NAME, index));
            //PlayerPrefs.SetString(ARCHIVE_SUMMARY_PREFIX + index, summaryInfo);
        }
        
        //载入存档
        public static bool DoLoadGame(int index)
        {
            //ES3取得存档数据
            string path = string.Format(ARCHIVE_FILE_NAME, index);
            var r =  ES3.Load<GameRuntimeData>(nameof(GameRuntimeData), path);
            _instance = r;
            
            if (r == null)
            {
                return false;
            }

            //将存档的角色数据给当前角色实例
            GameRuntimeData.Instance.AllRoles = r.AllRoles;
            
            var loadPara = new LevelMaster.LevelLoadPara() {loadType = LevelMaster.LevelLoadPara.LevelLoadType.Load};

            //加载地图
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

            LevelLoader.LoadGameMap(GameConfigDatabase.Instance.Get<Jyx2ConfigMap>(mapId), loadPara,
                () => { LevelMaster.Instance.TryBindPlayer(); });
            return true;
        }

        public static DateTime? GetSaveDate(int index)
		{
            var summaryInfoFilePath = string.Format(ARCHIVE_SUMMARY_FILE_NAME, index);
            return ES3.FileExists(summaryInfoFilePath) ?
                 ((DateTime?) ES3.GetTimestamp(summaryInfoFilePath)) : null;
        }

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
        
        private string GenerateSaveSummaryInfo()
        {
            string mapName = LevelMaster.GetCurrentGameMap().GetShowName();
            return $"{Player.Level}级,{mapName},队伍:{GetTeamMembersCount()}人";
        }

        #endregion
        
        #region 游戏运行时数据

        //主角
        public RoleInstance Player
        {
            get { return GetRole(0); }
        }


        //获取队伍所有角色
        public IEnumerable<RoleInstance> GetTeam()
        {
            foreach (var id in TeamId)
            {
                yield return AllRoles[id];
            }
        }

        //获取队伍人数
        public int GetTeamMembersCount()
        {
            return TeamId.Count;
        }

        public bool JoinRoleToTeam(int roleId,bool showGetItem = false)
        {
            if (GetRoleInTeam(roleId) != null)
            {
                Debug.LogError($"错误，角色重复入队：id = {roleId}");
                return false;
            }
            
            var role = GetRole(roleId);
            if(role == null)
            {
                Debug.LogError($"调用了不存在的role加入队伍，id = {roleId}");
                return false;
            }
            
            //获得角色身上的道具
            foreach (var item in role.Items)
            {
                if (!ItemAdded.Contains(item.Item.Id))
                {
                    if (item.Count == 0) item.Count = 1;
                    AddItem(item.Item.Id, item.Count);
                    ItemAdded.Add(item.Item.Id);
                    if (item.Count > 0 && showGetItem)
                    {
                        StoryEngine.Instance.DisplayPopInfo("得到物品:".GetContent(nameof(GameRuntimeData)) + item.Item.Name + "×" + Math.Abs(item.Count));
                    }
                    item.Count = 0;
                }
            }

            //清空角色身上的装备和物品
            /*role.Equipments.Clear();
            role.Items.Clear();*/
            
            TeamId.Add(roleId);
            return true;
        }

        public bool LeaveTeam(int roleId) 
        {
            var role = GetRole(roleId);
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

            //卸下角色身上的装备，清空修炼
            role.UnequipItem(role.Equipments[0],0);
            role.UnequipItem(role.Equipments[1],1);
            role.UnequipItem(role.Equipments[2],2);
            role.UnequipItem(role.Equipments[3],3);

            TeamId.Remove(roleId);
            role.Recover(true);
            return true;
        }

        public RoleInstance GetRoleInTeam(int roleId)
        {
            if (TeamId.Contains(roleId))
            {
                return AllRoles[roleId];
            }

            return null;
        }

        public bool IsRoleInTeam(int roleId)
        {
            return TeamId.Contains(roleId);
        }



        public RoleInstance GetRole(int roleId)
        {
            return AllRoles[roleId];
        }



        #region KeyValues

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



        public int GetMoney()
        {
            return GetItemCount(GameConst.MONEY_ID);
        }
        


        public bool HaveItemBool(int itemId)
        {
            return Items.ContainsKey(itemId.ToString());
        }

        //JYX2增加物品
        public void AddItem(string id, int count)
        {
            if (!Items.ContainsKey(id))
            {
                if(count < 0)
                {
                    Debug.LogError("扣了不存在的物品,id=" + id + ",count=" + count);
                    return;
                }
                Items[id] = count;
            }
            else
            {
                Items[id] += count;
                if(Items[id] == 0)
                {
                    Items.Remove(id);
                }else if(Items[id] < 0)
                {
                    Debug.LogError("物品扣成负的了,id=" + id + ",count=" + count);
                    Items.Remove(id);
                }
            }
        }

        //JYX2增加物品
        public void AddItem(int id, int count)
        {
            AddItem(id.ToString(), count);
        }


        public int GetItemCount(int id)
        {
            if (Items.ContainsKey(id.ToString()))
                return Items[id.ToString()];
            return 0;
        }

        public void RoleGetItem(int roleId, int itemId, int count)
        {
            //实现的逻辑是，如果这个角色在队里，则直接加到角色身上。否则加到一个存档记录里，在之后生成这个角色的时候再进行添加
            var teamRole = GetRole(roleId);
            if (teamRole != null)
            {
                teamRole.AddItem(itemId, count);
            }
        }

        //设置物品使用人
        public void SetItemUser(int itemId, int roleId)
        {
            ItemUser[itemId.ToString()] = roleId;
        }

        //获取物品使用人
        public int GetItemUser(int id)
        {
            if (ItemUser.ContainsKey(id.ToString()))
                return ItemUser[id.ToString()];
            return -1;
        }


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
            var gameMap = Jyx2ConfigMap.Get(mapId);
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
