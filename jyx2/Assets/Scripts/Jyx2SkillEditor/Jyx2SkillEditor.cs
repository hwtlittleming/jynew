

using UnityEngine;
using Jyx2;
using Sirenix.OdinInspector;

public class Jyx2SkillEditor : MonoBehaviour
{
    public MapRole player;

    public Jyx2SkillEditorEnemy[] enemys;

    public Transform[] faceTrans;
    public Transform[] lineTrans;
    public Transform[] crossTrans;

    [LabelText("是否测试左右互搏")]
    public bool TestZuoyouhubo = false;
    // Start is called before the first frame update
    async void Start()
    {
        /*
        FileSystemWatcher watcher;

        //监控excel文件夹
        watcher = new FileSystemWatcher();
        watcher.BeginInit();
        watcher.Path = "excel";
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.Size;
        watcher.Changed += Watcher_Changed;
        watcher.EndInit();
        */


        player.IsInBattle = true;
        //Container.TryResolve<IXLsReloader>()?.Do();

        await BeforeSceneLoad.loadFinishTask;
        
        await Jyx2_UIManager.Instance.ShowUIAsync(nameof(SkillEditorUIPanel),player,enemys);
    }

    /*
    private void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        updateExcel = true;
    }

    bool updateExcel = false;
    */



    // Update is called once per frame
    void Update()
    {
      

        /*
        if (updateExcel)
        {
            updateExcel = false;
            Container.TryResolve<IXLsReloader>()?.Do();
        }
        */

      
    }

    /// <summary>
    /// 预览技能
    /// </summary>
    /// <param name="skillName"></param>
    public void PreviewSkill(string skillName)
    {
        var skillEditorUIPanel = FindObjectOfType<SkillEditorUIPanel>();
        skillEditorUIPanel.SwitchToSkill(skillName);
    }
}
