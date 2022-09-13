using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "配置文件/商店", fileName = "商店配置")]
    public class ConfigShop : ConfigBase
    {
        [LabelText("韦小宝触发器名")] 
        public int Trigger;

        [LabelText("商品列表")][TableList] 
        public List<ConfigShopItem> ShopItems;

        public override async UniTask WarmUp()
        {
            
        }
    }
    
    [Serializable]
    public class ConfigShopItem
    {
        [LabelText("道具")] [SerializeReference] [InlineEditor]
        public ItemInstance Item;

        [LabelText("数量")] public int Count;

        [LabelText("价格")] public int Price;
    }
}