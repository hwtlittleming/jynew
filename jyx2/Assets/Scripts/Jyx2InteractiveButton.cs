
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Jyx2
{
    public static class Jyx2InteractiveButton
    {
        public static void Show(string text, Action callback)
        {
            var btn = GetInteractiveButton();
            if(btn != null)
            {
                btn.GetComponentInChildren<Text>().text = text;
                btn.gameObject.SetActive(true);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => { callback(); });
                btn.transform.Find("FocusImage").gameObject.SetActive(true);
            }
        }

        public static void Hide()
        {
            var btn = GetInteractiveButton();
            if(btn != null)
            {
                btn.gameObject.SetActive(false);
            }
        }

        public static Button GetInteractiveButton()
        {
            var root = GameObject.Find("LevelMaster/UI");
            var btn = root.transform.Find("InteractiveButton").GetComponent<Button>();
            return btn;
        }
    }
}
