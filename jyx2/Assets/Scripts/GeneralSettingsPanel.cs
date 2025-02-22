﻿

using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;

public class GeneralSettingsPanel : UIBase
{
    public Dropdown resolutionDropdown;
    public Dropdown windowDropdown;
    public Dropdown difficultyDropdown;
    public Dropdown viewportDropdown;
    public Dropdown languageDropdown;

    public Slider volumeSlider;
    public Slider soundEffectSlider;

    public Button m_CloseButton;

    private GraphicSetting _graphicSetting;
    Resolution[] resolutions;

    private UnityEvent<float> OnVolumeChange;
    
    private Dictionary<GameSettingManager.Catalog, UnityEvent<object>> _gameSettingEvents;

    private Dictionary<GameSettingManager.Catalog, object> gameSetting => GameSettingManager.settings;

    private void Awake()
    {
        //读取语言文件
        var langPath = Path.Combine(Application.streamingAssetsPath, "Language");
        if (Directory.Exists(langPath)) Directory.CreateDirectory(langPath);//安全性检查
        var languageOptions = new List<Dropdown.OptionData>();
        //绑定到指定的文件夹目录
        var langDir = new DirectoryInfo(langPath);

        if (!langDir.Exists)
            return;

        //检索表示当前目录的文件和子目录
        var fsinfos = langDir.GetFileSystemInfos();
        //遍历检索的文件和子目录
        for (var index = 0; index < fsinfos.Length; index++)
        {
            var fsinfo = fsinfos[index];
            if (fsinfo is FileInfo && fsinfo.Extension == ".json")
            {
                languageOptions.Add(new Dropdown.OptionData(fsinfo.Name.Replace(".json", "")));
            }
        }

        languageDropdown.AddOptions(languageOptions);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GeneralSettingsPanel Start()");
        _graphicSetting = GraphicSetting.GlobalSetting;

        InitWindowDropdown();
        InitResolutionDropdown();
        InitVolumeSlider();
        InitSoundEffectSlider();
        InitViewportSetting();
        InitLanguageSetting();
        
        windowDropdown.onValueChanged.AddListener(SetFullscreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        volumeSlider.onValueChanged.AddListener(SetVolume);
        soundEffectSlider.onValueChanged.AddListener(SetSoundEffect);
        viewportDropdown.onValueChanged.AddListener(SetViewport);
        languageDropdown.onValueChanged.AddListener(SetLanguage);
        
        m_CloseButton.onClick.AddListener(Close);
        
        Debug.Log("GeneralSettingsPanel Start() END");
    }

    public void Close()
    {
        _graphicSetting.Save();
        _graphicSetting.Execute();
        UIManager.Instance.HideUI(nameof(GameSettingsPanel));
    }

    public void InitResolutionDropdown()
    {
        var setting = (string) gameSetting[GameSettingManager.Catalog.Resolution];

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentIndex = 0;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;

            if (!options.Contains(option))
            {
                //如果是当前的分辨率，则记下来
                if (option.Equals(setting))
                {
                    currentIndex = i;
                }
                
                options.Add(option);
            }
                
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void InitWindowDropdown()
    {
#if !UNITY_ANDROID
        var setting = (int) gameSetting[GameSettingManager.Catalog.Fullscreen];
        windowDropdown.value = setting;
        windowDropdown.RefreshShownValue();
#endif
    }

    public void InitDifficultyDropdown()
    {
    }

    void InitVolumeSlider()
    {
        var volume = gameSetting[GameSettingManager.Catalog.Volume];
        if (volume is float value)
        {
            volumeSlider.value = value;
        }
    }

    public void InitSoundEffectSlider()
    {
        var volume = gameSetting[GameSettingManager.Catalog.SoundEffect];
        if (volume is float value)
        {
            soundEffectSlider.value = value;
        }
    }
    
   private void InitViewportSetting()
    {
        var setting = gameSetting[GameSettingManager.Catalog.Viewport];
        if (setting is int value)
        {
            viewportDropdown.value = value;
        }
    }

   
   private void InitLanguageSetting()
   {
       var setting = gameSetting[GameSettingManager.Catalog.Language];
       if (setting is int value)
       {
           languageDropdown.value = value;
       }
   }

    private void SetResolution(int index)
    {
        GameSettingManager.UpdateSetting(GameSettingManager.Catalog.Resolution, index);
    }

    private void SetVolume(float volume)
    {
        GameSettingManager.UpdateSetting(GameSettingManager.Catalog.Volume, volume);
    }

    private void SetSoundEffect(float volume)
    {
        GameSettingManager.UpdateSetting(GameSettingManager.Catalog.SoundEffect, volume);
    }

    private void SetFullscreen(int index)
    {
        GameSettingManager.UpdateSetting(GameSettingManager.Catalog.Fullscreen, index);
    }

    private void SetViewport(int index)
    {
        GameSettingManager.UpdateSetting(GameSettingManager.Catalog.Viewport, index);
    }
    
    private void SetLanguage(int index)
    {
        GameSettingManager.UpdateSetting(GameSettingManager.Catalog.Language, languageDropdown.options[index].text);
    }

    /*游戏难度，暂未实现*/
    public void SetGameDifficulty(int index)
    {

    }

    protected override void OnCreate()
    {

    }

    public override void Update()
    {
        //only allow close setting for now, so at least this UI can be closed via gamepad
        if (gameObject.activeSelf)
        {
            if (GamepadHelper.IsConfirm() || GamepadHelper.IsCancel())
            {
                Close();
            }
        }
    }
}
