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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ch.sycoforge.Decal;
using Cysharp.Threading.Tasks;
using Jyx2;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleboxManager : MonoBehaviour
{
    //排除拥挤点半径
    public float m_DetechRadius = 0.8f;
    public float m_SpriteToGroundHeight = 0.01f;
    public Color m_InvalidColor = new Color(1,1,1,0.2f);
    public const float BATTLEBLOCK_DECAL_ALPHA = 0.4f;

    private SpriteRenderer _BlockPrefab;

    [HideInInspector]
    public BattleboxDataset m_Dataset;

    private Collider[] _colliders;

    //存储逻辑数据
    private List<BattleBlockData> _battleBlocks = new List<BattleBlockData>();
    
    //mouseover显示攻击范围的格子
    private List<BattleBlockData> _rangeLayerBlocks = new List<BattleBlockData>();
    
    private GameObject _parent;

    // Use this for initialization
    void Awake()
    {
        Init();
    }

    public void Init()
    {
        InitCollider();
        
    }

    private void InitCollider()
    {
        _colliders = GetComponentsInChildren<Collider>();
        foreach (var col in _colliders)
        {
            var mesh = col.GetComponent<MeshCollider>();
            if (mesh != null) mesh.convex = true;
        }
    }

    public List<BattleBlockData> GetBattleBlocks()
    {
        return _battleBlocks;
    }

    public System.Numerics.Vector2 GetXYIndex(float x, float z)
    {
        return m_Dataset.GetXYIndex(x, z);
    }
    

    public void ShowAllValidBlocks()
    {
        foreach (var block in _battleBlocks)
        {
            block.Show();
        }
    }

    public void HideAllBlocks()
    {
        foreach (var block in _battleBlocks)
        {
            block.Hide();
        }
    }

    public void HideAllRangeBlocks()
    {
        foreach (var block in _rangeLayerBlocks)
        {
            block.Hide();
        }
    }

    public BattleBlockData GetBlockData(int xindex, int yindex)
    {
        return _battleBlocks.FirstOrDefault(x => x.BattlePos.X == xindex && x.BattlePos.Y == yindex);
    }
    
    public BattleBlockData GetRangelockData(int xindex, int yindex)
    {
        return _rangeLayerBlocks.FirstOrDefault(x => x.BattlePos.X == xindex && x.BattlePos.Y == yindex);
    }

    public bool Exist(int xindex, int yindex)
    {
        return m_Dataset.Exist(xindex, yindex);
    }

    //清除所有格子（所有格子的parent为当前box）
    public void ClearAllBlocks()
    {
        foreach (var block in _battleBlocks)
        {
            DestroyImmediate(block.gameObject);
        }
        _battleBlocks.Clear();
        _rangeLayerBlocks.Clear();

        var parent = FindOrCreateBlocksParent();
        if (parent == null) return;
        DestroyImmediate(parent);
    }

    private GameObject FindOrCreateBlocksParent()
    {
        if (_parent == null)
            _parent = new GameObject("block_parent");
        return _parent;
    }

    public void SetAllBlockColor(Color color, bool isRangeBlocks = false)
    {
        foreach (var block in isRangeBlocks ? _rangeLayerBlocks : _battleBlocks)
		{
			setBlockColor(color, block);
			//block.gameObject.GetComponent<EasyDecal>().DecalMaterial.SetColor("_TintColor", color);
		}
	}

	private void setBlockColor(Color color, BattleBlockData block)
	{
		block.gameObject.GetComponent<EasyDecal>().DecalRenderer.material.SetColor("_TintColor", color);
	}

    public void SetBlockInaccessible(BattleBlockData block)
	{
        setBlockColor(new Color(0, 0, 0, 0), block);
	}
    
    //y是奇数
    //static readonly int[] dx_odd = new int[] { 1, 1, 1, 0, -1, 0 };

    ////y是偶数
    //static readonly int[] dx_even = new int[] { 0, 1, 0, -1, -1, -1 };
    //static readonly int[] dy = new int[] { 1, 0, -1, -1, 0, 1 };


    static readonly int[] dx = new int[] { 1, 0, -1, 0 };
    static readonly int[] dy = new int[] { 0, 1, 0, -1 };
}
