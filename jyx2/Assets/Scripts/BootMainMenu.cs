using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootMainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.GameStart();
    }
}
