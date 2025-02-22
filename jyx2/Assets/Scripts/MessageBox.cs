
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Jyx2;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    public Button m_ConfirmButton;
    public Text m_MessageText;
	private Action _callback;

	public static void Create(string msg, Action callback, Transform parent = null)
    {
        if(parent == null)
        {
            var go = GameObject.Find("MainUI");
            parent = go.transform;
        }

        var obj = Jyx2ResourceHelper.CreatePrefabInstance("MessageBox");
        obj.transform.SetParent(parent);

        var rt = obj.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;

        var messageBox = obj.GetComponent<MessageBox>();
        messageBox.Show(msg, callback);
    }

    public void Show(string msg, Action callback)
    {
        m_MessageText.text = msg;
        _callback = callback;
        m_ConfirmButton.onClick.RemoveAllListeners();
		m_ConfirmButton.onClick.AddListener(() =>
		{
			closeAndCallback();
		});
    }

	private void closeAndCallback()
	{
		Jyx2ResourceHelper.ReleasePrefabInstance(this.gameObject);
		if (_callback != null)
			_callback();
	}

	private void Update()
	{
        if (gameObject.activeSelf)
            if (GamepadHelper.IsConfirm()
                || GamepadHelper.IsCancel())
                closeAndCallback();
	}
}
