using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingsPanel : UIBase
{
    public Transform GeneralSettingsPanel;
    public Transform GraphicPanel;

    // Start is called before the first frame update
    void Start()
    {
        GeneralSettingsPanel.gameObject.SetActive(true);        
        GraphicPanel.gameObject.SetActive(false);
        
        GlobalHotkeyManager.Instance.RegistHotkey(this, KeyCode.Escape,
            GeneralSettingsPanel.GetComponent<GeneralSettingsPanel>().Close);
    }

    private void OnDestroy()
    {
        GlobalHotkeyManager.Instance.UnRegistHotkey(this, KeyCode.Escape);
    }


    // Update is called once per frame
    protected override void OnCreate()
    {
        
    }

    void Update()
    {
        
    }
}