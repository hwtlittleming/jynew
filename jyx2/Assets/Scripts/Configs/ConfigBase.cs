using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [Serializable] 
    abstract public class ConfigBase : ScriptableObject
    {
        protected const string DEFAULT_GROUP_NAME = "基本配置";
        
        [BoxGroup(DEFAULT_GROUP_NAME)][LabelText("ID")] 
        public int Id;
        
        [BoxGroup(DEFAULT_GROUP_NAME)][LabelText("名称")] 
        public string Name;
        
        /// 资源预热
        public abstract UniTask WarmUp();
    }
}