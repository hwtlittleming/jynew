
using Jyx2;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Animancer;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    /// <summary>
    /// 交互的视野范围
    /// </summary>
    const float PLAYER_INTERACTIVE_RANGE = 1f;

    /// <summary>
    /// 交互的视野角度
    /// </summary>
    const float PLAYER_INTERACTIVE_ANGLE = 120f;

    /// <summary>
    /// 是否激活交互选项
    /// </summary>
    [HideInInspector]
    public bool EnableInteractive { get; set; }

    private bool canControl = true;
    
    public static Player GetPlayer()
    {
        if (LevelMaster.Instance == null)
            return null;
        
        return LevelMaster.Instance.GetPlayer();
    }
    public HybridAnimancerComponent m_Animancer;
    public Animator m_Animator;

    
    public bool IsOnBoat;

    NavMeshAgent _navMeshAgent;
    Jyx2Boat _boat;

    GameEventManager evtManager
    {
        get
        {
            if(_evtManager == null)
            {
                _evtManager = FindObjectOfType<GameEventManager>();
            }
            return _evtManager;
        }
    }

    GameEventManager _evtManager;

    public void GetInBoat(Jyx2Boat boat)
    {
        IsOnBoat = true;
        _boat = boat;

        _navMeshAgent.updatePosition = false;
        transform.position = boat.transform.position;
        transform.rotation = boat.transform.rotation;

        SetHide(true);
        _navMeshAgent.areaMask = GetWaterNavAreaMask();
		_navMeshAgent.Warp(boat.transform.position);
        _navMeshAgent.updatePosition = true;
    }

    public bool GetOutBoat()
    {
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(transform.position, out myNavHit, 3.5f, GetNormalNavAreaMask()))
        {
            //比水平面还低
            if (myNavHit.position.y < 5f)
            {
                return false;
            }

            SetHide(false);
            _navMeshAgent.areaMask = GetNormalNavAreaMask();
            IsOnBoat = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    void SetHide(bool isHide)
    {
        foreach (var r in transform.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            r.enabled = !isHide;
        }
    }

    /// <summary>
    /// 获取水路行走mask
    /// </summary>
    /// <returns></returns>
    int GetWaterNavAreaMask()
    {
        return (0 << 0) + (0 << 1) + (1 << 2) + (1 << 3);
    }

    /// <summary>
    /// 获取普通的陆地行走mask
    /// </summary>
    /// <returns></returns>
    int GetNormalNavAreaMask()
    {
        return (1 << 0) + (0 << 1) + (1 << 2) + (0 << 3);
    }


    public void Init()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _boat = FindObjectOfType<Jyx2Boat>();

        EnableInteractive = true;
    }

    void Start()
    {
        Init();
        
        //修复一些场景里有主角贴图丢失导致紫色的情况
        if (GetComponent<SkinnedMeshRenderer>() != null)
        {
            GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
    }

    public void CanControl(bool isOn)
    {
        canControl = isOn;
    }

    void Update()
    {
        //在船上
        if (IsOnBoat)
        {
            _boat.transform.position = this.transform.position;
            _boat.transform.rotation = this.transform.rotation;
        }

        if (!canControl)
            return;

        BigMapIdleJudge();
        
        //判断交互范围
        Debug.DrawRay(transform.position, transform.forward, Color.yellow);
        
        //获得当前可以触发的交互物体,离开交互物体 将UI去除
        var gameEvent = DetectInteractiveGameEvent();
        if (gameEvent == null)
        {
            evtManager.OnExitAllEvents();
        }
    }


    private float _bigmapIdleTimeCount = 0;
    private const float BIG_MAP_IDLE_TIME = 5f;
    private bool _playingbigMapIdle = false;

    private Animator _playerAnimator;
    private HybridAnimancerComponent _playerAnimancer;

    private Animator GetPlayerAnimator()
    {
        if (_playerAnimator == null)
            _playerAnimator = this.transform.GetChild(0).GetComponent<Animator>();
        return _playerAnimator;
    }
    
    private HybridAnimancerComponent GetPlayerAnimancer()
    {
        if (_playerAnimancer == null)
            _playerAnimancer = GameUtil.GetOrAddComponent<HybridAnimancerComponent>(this.transform.GetChild(0));
        return _playerAnimancer;
    }
    
    //在大地图上判断是否需要展示待机动作
    void BigMapIdleJudge()
    {
        if(_boat == null) return; //暂实现：判断是否是大地图，有船才是大地图

        var animator = GetPlayerAnimator();
        
        if (_playingbigMapIdle)
        {
            //判断是否有移动速度，有的话立刻打断目前IDLE动作
            if (animator!=null && animator.GetFloat("speed") > 0)
            {
                var animancer = GetPlayerAnimancer();
                animancer.Stop();
                animancer.PlayController();
                _playingbigMapIdle = false;
            }
            return;
        }

        //一旦开始移动，则重新计时
        if (animator!=null && animator.GetFloat("speed") > 0)
        {
            _bigmapIdleTimeCount = 0;
            return;
        }
        
        _bigmapIdleTimeCount += Time.deltaTime;
        if (_bigmapIdleTimeCount > BIG_MAP_IDLE_TIME)
        {
            //展示IDLE动作
            _bigmapIdleTimeCount = 0;
            var animancer = GetPlayerAnimancer();
            var clip = Jyx2.Middleware.Tools.GetRandomElement(GlobalAssetConfig.Instance.bigMapIdleClips);
            animancer.Play(clip, 0.25f);
            _playingbigMapIdle = true;
        }
    }
    
    private Collider[] targets = new Collider[10];
    
    /// 在交互视野范围内寻找第一个可被交互物体
    GameEvent DetectInteractiveGameEvent()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, PLAYER_INTERACTIVE_RANGE, targets, LayerMask.GetMask("GameEvent"));
        //添加
        for (int i = 0; i < count; i++)
        {
            var target = targets[i];
            var evt = target.GetComponent<GameEvent>();
            if (evt == null) return null;
            if (evt.m_EventType.Contains("交互") || evt.m_EventType.Contains("观察") || evt.m_EventType.Contains("使用物品") 
                || evt.m_EventType.Contains("偷袭)"))
            {
                //找到第一个可交互的物体，则结束
                return target.GetComponent<GameEvent>();
            }
        }
        return null;
    }

    //保存世界信息
    public void RecordWorldInfo()
    {
        var runtime = GameRuntimeData.Instance;

        if (runtime.WorldData == null)
        {
            runtime.WorldData = new WorldMapSaveData();
        }

        WorldMapSaveData worldData = runtime.WorldData;
        worldData.WorldPosition = this.transform.position;
        worldData.WorldRotation = this.transform.rotation;
        worldData.BoatWorldPos = _boat.transform.position;
        worldData.BoatRotate = _boat.transform.rotation;
        worldData.OnBoat = IsOnBoat ? 1 : 0;
    }

    public void LoadWorldInfo()
    {
        var runtime = GameRuntimeData.Instance;
        if (runtime.WorldData == null) return;
        
        PlayerSpawnAt(runtime.WorldData.WorldPosition, runtime.WorldData.WorldRotation);

        LoadBoat();

        if (runtime.WorldData.OnBoat == 1)
        {
            _boat.GetInBoat();
        }
    }

    public void LoadBoat()
    {
        var runtime = GameRuntimeData.Instance;
        if (runtime.WorldData == null)
            return; //首次进入
        
        _boat.transform.position = runtime.WorldData.BoatWorldPos;
        _boat.transform.rotation = runtime.WorldData.BoatRotate;
    }

    public Vector3 GetBoatPosition()
    {
        return _boat == null ? new Vector3() : _boat.transform.position;
    }

    void PlayerSpawnAt(Vector3 spawnPos,Quaternion ori)
    {
        _navMeshAgent.enabled = false;
        Debug.Log("load pos = " + spawnPos);
        transform.position = spawnPos;
		transform.rotation = ori;
        _navMeshAgent.enabled = true;
    }
}