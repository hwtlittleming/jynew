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
using System.Threading;
using System.Threading.Tasks;
using ch.sycoforge.Decal;
using Jyx2;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class BattleboxHelper : MonoBehaviour
{
	public const float BATTLEBLOCK_DECAL_ALPHA = 0.4f;
	public static BattleboxHelper Instance
	{
		get
		{
			if (_instance == null) _instance = FindObjectOfType<BattleboxHelper>();
			return _instance;
		}
	}
	private static BattleboxHelper _instance;

	//绘制区域（主角身边的范围）
	public int m_MoveZoneDrawRange = 16;

	private BattleboxManager _currentBattlebox;

	private const string RootPath = "BattleboxRoot";
	private bool _isInit = false;
	private BattleboxManager[] _boxList;
	private GameObject _boxRoot;
	private bool downDpadPressed;
	private bool currentlyReleased = true;
	private bool upDpadPressed;

	void Start()
	{
		Init();
	}

	public void initBattleBlockData()
	{
		//初始化格子
		Transform all_block = GameObject.Find("block_parent").transform;
		int maxX = 1;
		int maxY = 1;
		foreach (Transform block in all_block)
		{
			BattleBlockData b = new BattleBlockData();
			b.block = block.gameObject;
			b.WorldPos = block.position;
			b.team = block.name.Split('-')[0];
			b.x = int.Parse(block.name.Split('-')[1]);
			b.y = int.Parse(block.name.Split('-')[2]);
			b.blockName = block.name;
			if(maxX < b.x) b.maxX = b.x;
			if(maxY < b.y) b.maxX = b.y;
			BattleManager.Instance.block_list.Add(b);
		}
        //每个格子都记录整个生成的所有格子的最大长和宽
		foreach (var block in BattleManager.Instance.block_list)
		{
			block.maxX = maxX;
			block.maxY = maxY;
		}
	}
	
	public BattleBlockData GetBlockData(int xindex, int yindex)
	{
		if (!GeneralPreJudge()) return null;

		return _currentBattlebox.GetBlockData(xindex, yindex);
	}

	//清除当前
	//脱离战斗的时候必须调用
	public void ClearAllBlocks()
	{
		if (_isInit && _currentBattlebox != null)
			_currentBattlebox.ClearAllBlocks();
	}

	//获取坐标对应的格子
	public BattleBlockData GetLocationBattleBlock(Vector3 pos)
	{
		var tempXY = _currentBattlebox.GetXYIndex(pos.x, pos.z);
		var centerX = (int)tempXY.X;
		var centerY = (int)tempXY.Y;
		return GetBlockData(centerX, centerY);
	}

	//判断格子是否存在（必须是有效格子）
	public bool IsBlockExists(int xindex, int yindex)
	{
		if (!GeneralPreJudge()) return false;

		if (!_currentBattlebox.Exist(xindex, yindex)) return false;

		var block = _currentBattlebox.GetBlockData(xindex, yindex);
		if (block != null) return false;

		return true;
	}

	private int[] xPositions = new int[0];
	private int xMiddlePos = -1;
	private int[] yPositions = new int[0];
	private int yMiddlePos;
	private int xCurPos;
	private int yCurPos;

	public bool AnalogMoved = false;

	private void Update()
	{
		if (xPositions.Length == 0 || yPositions.Length == 0)
			return;

		var move = GamepadHelper.GetLeftAnalogMove();
		var leftStickX = move.X;
		var leftStickY = move.Y;

		if (Math.Abs(leftStickX) > 0 || Math.Abs(leftStickY)> 0)
		{
			if (leftStickY < 0)
			{
				if (currentlyReleased)
				{
					if (yCurPos < yPositions.Last())
					{
						yCurPos++;

						if (!setSelectedBlock())
							yCurPos--;
					}
				}
			}
			else if (leftStickY > 0)
			{
				if (currentlyReleased)
				{
					if (yCurPos > yPositions.First())
					{
						yCurPos--;

						if (!setSelectedBlock())
							yCurPos++;
					}
				}
			}

			if (leftStickX < 0)
			{
				if (currentlyReleased)
				{
					if (xCurPos < xPositions.Last())
					{
						xCurPos++;

						if (!setSelectedBlock())
							xCurPos--;
					}
				}
			}
			else if (leftStickX > 0)
			{
				if (currentlyReleased)
				{
					if (xCurPos > xPositions.First())
					{
						xCurPos--;

						if (!setSelectedBlock())
							xCurPos++;
					}
				}
			}

			currentlyReleased = false;
			delayedAxisRelease();
		}

		if (GamepadHelper.IsConfirm())
		{
			if (AnalogMoved && blockConfirmed != null)
			{
				var selectedBlock =  _currentBattlebox.GetBlockData(xCurPos, yCurPos);
				if (selectedBlock != null && !selectedBlock.Inaccessible)
				{
					blockConfirmed(selectedBlock);
				}
			}
		}
	}

	private bool setSelectedBlock()
	{
		var newSelectedBlock =  _currentBattlebox.GetBlockData(xCurPos, yCurPos);
		if (newSelectedBlock != null && newSelectedBlock.IsActive)
		{
			if (_selectedBlock != null)
			{
				_selectedBlock.gameObject.GetComponent<EasyDecal>().DecalRenderer.material.SetColor("_TintColor", _oldColor);
			}

			_selectedBlock = newSelectedBlock;
			_oldColor = newSelectedBlock.gameObject.GetComponent<EasyDecal>().DecalRenderer.material.GetColor("_TintColor");
			Color hiliteColor = newSelectedBlock.Inaccessible ?				
				new Color(0.4f, 0.4f, 0.4f, BattleboxManager.BATTLEBLOCK_DECAL_ALPHA) : //gray color for inaccessible blocks
				new Color(1, 0, 1, BattleboxManager.BATTLEBLOCK_DECAL_ALPHA);
			_selectedBlock.gameObject.GetComponent<EasyDecal>().DecalRenderer.material.SetColor("_TintColor", hiliteColor);

			AnalogMoved = true;

			if (analogLeftMovedToBlock != null)
				analogLeftMovedToBlock(newSelectedBlock);

			return true;
		}

		return false;
	}

	public event Action<BattleBlockData> analogLeftMovedToBlock;
	public event Action<BattleBlockData> blockConfirmed;

	private void initXPos()
	{
		xPositions = this._currentBattlebox
		.GetBattleBlocks()
		.Where(b => b.IsActive)
		.Select(b => b.BattlePos.X)
		.OrderBy(p => p)
		.Distinct()
		.ToArray();
	}

	private void initYPos()
	{
		yPositions = this._currentBattlebox
		.GetBattleBlocks()
		.Where(b => b.IsActive)
		.Select(b => b.BattlePos.Y)
		.OrderBy(p => p)
		.Distinct()
		.ToArray();
	}

	protected void delayedAxisRelease()
	{
		Task.Run(() =>
		{
			Thread.Sleep(200);
			currentlyReleased = true;
		});
	}

	bool rangeMode = false;
	private BattleBlockData _selectedBlock;
	private Color _oldColor;

	public void ShowBlocks(RoleInstance role, IEnumerable<BattleBlockVector> list, BattleBlockType type = BattleBlockType.MoveZone,
		bool selectMiddlePos = false)
	{
		if (!GeneralPreJudge()) return;
		HideAllBlocks();

		if (type == BattleBlockType.MoveZone)
		{
			_currentBattlebox.SetAllBlockColor(new Color(1, 1, 1, BattleboxManager.BATTLEBLOCK_DECAL_ALPHA));
			_selectedBlock = null;
		}
		else if (type == BattleBlockType.AttackZone)
		{
			_currentBattlebox.SetAllBlockColor(new Color(1, 0, 0, BattleboxManager.BATTLEBLOCK_DECAL_ALPHA));
			_selectedBlock = null;
		}

		foreach (var vector in list)
		{
			var block = _currentBattlebox.GetBlockData(vector.X, vector.Y);
			if (block != null)
			{
				if (vector.Inaccessible)
				{
					_currentBattlebox.SetBlockInaccessible(block);
				}

				block.Inaccessible = vector.Inaccessible;
				block.Show();
			}
		}

		xMiddlePos = role.Pos.X;
		yMiddlePos = role.Pos.Y;
		initShownPositions();

		if (selectMiddlePos)
		{
			setSelectedBlock();
		}

		rangeMode = false;
	}

	private void initShownPositions()
	{
		initXPos();
		xCurPos = xMiddlePos;

		initYPos();
		yCurPos = yMiddlePos;
	}

	public void ShowRangeBlocks(IEnumerable<BattleBlockVector> list)
	{
		//todo: debug zhaoshi that has range instead of just one point
		if (!GeneralPreJudge()) return;
		_currentBattlebox.HideAllRangeBlocks();

		foreach (var vector in list)
		{
			var block = _currentBattlebox.GetRangelockData(vector.X, vector.Y);
			if (block != null) 
				block.Show();
		}

		//initShownPositions();
		rangeMode = true;
	}

	public void HideAllBlocks(bool hideRangeBlock = false)
	{
		if (!GeneralPreJudge()) return;

		_currentBattlebox.HideAllBlocks();
		if (hideRangeBlock)
		{
			_currentBattlebox.HideAllRangeBlocks();
		}

		_selectedBlock = null;
	}



	public void ShowAllBlocks()
	{
		if (!GeneralPreJudge()) return;

		_currentBattlebox.ShowAllValidBlocks();
	}

	private void Init()
	{
		_boxRoot = GameObject.Find(RootPath);
		if (_boxRoot == null)
		{
			Debug.Log($"当前场景找不到Battlebox的根节点(BattleboxRoot)，初始化失败,本场景无法战斗！");
			return;
		}

		_boxList = _boxRoot.GetComponentsInChildren<BattleboxManager>();
		if (_boxList == null || _boxList.Length == 0)
		{
			Debug.Log($"当前场景BattleboxRoot节点下没有Battlebox，本场景无法战斗！");
			return;
		}

		_isInit = true;
	}

	private bool GeneralPreJudge()
	{
		if (!_isInit)
		{
			Debug.Log($"BattleboxHelper还没有初始化成功");
			return false;
		}

		if (_currentBattlebox == null)
		{
			Debug.Log($"BattleboxHelper没找到当前格子");
			return false;
		}
		return true;
	}
	public enum BattleBlockType
	{
		MoveZone = 0,
		AttackZone = 1,
	}
}
