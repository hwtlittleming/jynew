
using System.Collections;
using System.Collections.Generic;
using Jyx2;
using UnityEngine;

/// <summary>
/// 随着游戏存档进行调整
/// </summary>
public class FixWithGameRuntime : MonoBehaviour
{
    public enum FixTypeEnum
    {
        Move,
    }

    public FixTypeEnum FixType = FixTypeEnum.Move;
    public string Flag;
    public Transform MoveTo;
	private Vector3 storeP;//记录原始位置，清除flag之后复位。
	private Quaternion storeR;

    public void Reload()
    {
        var runtime = GameRuntimeData.Instance;
		if(storeP==new Vector3(0,0,0))
		{
			storeP=transform.position;
			storeR=transform.rotation;
		}
        if (runtime != null && runtime.KeyExist(Flag))
        {
            if (runtime.KeyValues[Flag]=="1")
            {
                if (MoveTo == null)
                {
                    Debug.LogError("FixWithGameRuntime 未定义的移动目标：");
                }
                else
                {
                    transform.position = MoveTo.position;
                    transform.rotation = MoveTo.rotation;
                }
            }
			else
			{
				transform.position = storeP;
                transform.rotation = storeR;
			}
        }
    }
}
