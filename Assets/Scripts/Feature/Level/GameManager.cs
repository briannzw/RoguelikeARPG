using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if(instance != null)
        {
            Debug.Log("There's more than one GameManager is detected.");
            return;
        }

        instance = this;
    }
    #endregion

    public LevelData LevelData;

    #region GUI
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 40), "Coins : " + LevelData.Coins.ToString());
    }
    #endregion
}
