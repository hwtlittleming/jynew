using System.Collections;
using System.Collections.Generic;
using Jyx2;
using UnityEngine;
using XNode;

[CreateNodeMenu("播放音乐或音效")]
[NodeWidth(150)]
public class PlayMusicNode : SimpleNode
{
    [Header("音乐id")]
    public int musicId;
    
    [Header("音效id")]
    public int waveId;
    
    private void Reset() {
        name = "播放音乐/音效";
    }

    protected override void DoExecute()
    {
        LuaBridge.PlayMusic(musicId);
        LuaBridge.PlayWave(waveId);
    }
}
