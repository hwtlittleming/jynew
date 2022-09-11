

using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Jyx2;
using Jyx2.Middleware;
using Jyx2.MOD;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class GameStart : MonoBehaviour
{
	public CanvasGroup introPanel;

	void Start()
	{
		StartAsync().Forget();
	}

	async UniTask StartAsync()
	{
		introPanel.gameObject.SetActive(true);

		introPanel.alpha = 0;
		await introPanel.DOFade(1, 1f).SetEase(Ease.Linear);
		await UniTask.Delay(TimeSpan.FromSeconds(1f));
		await introPanel.DOFade(0, 1f).SetEase(Ease.Linear).OnComplete(() =>
		{
			Destroy(introPanel.gameObject);
		});

		//直接进入游戏
		BeforeSceneLoad.ColdBind();
		SceneManager.LoadScene("0_MainMenu");
	}
}
