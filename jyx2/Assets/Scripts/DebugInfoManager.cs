
using UnityEngine;
using UnityEngine.UI;

public class DebugInfoManager : MonoBehaviour
{
    public static void Init()
    {
        var obj = FindObjectOfType<DebugInfoManager>();
        if (obj != null)
            return;

        //否则初始化
        var prefab = Resources.Load<GameObject>("DebugInfoManager");
        var newObj = Instantiate(prefab) as GameObject;
        newObj.name = "[DebugInfoManager]";
        DontDestroyOnLoad(newObj);
    }

    public Text m_FpsText;
    public float fps_updateInterval = 0.5F;

    private double lastInterval;
    private int frames = 0;
    private float fps;
    void CalcFps()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + fps_updateInterval)
        {
            fps = (float)(frames / (timeNow - lastInterval));
            frames = 0;
            lastInterval = timeNow;
        }
        m_FpsText.text = string.Format("FPS={0:f2}", fps);
    }

    // Update is called once per frame
    void Update()
    {
        CalcFps();
    }
}