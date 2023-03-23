using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (instance != null)
        {
            Debug.Log("There's more than one GameManager is detected.");
            return;
        }

        instance = this;
    }
    #endregion

    public bool GameEnded = false;
    [Header("References")]
    public TMP_Text TimerText;
    public GameObject EndPanel;
    public CinemachineInputProvider cinemachineInput;

    public Action GameTimerEnd;
    public Action GameEnd;
    public Action PlayerLose;

    [SerializeField] private LevelData levelData;
    private float timer;
    private TimeSpan ts;

    public Action OnCoinCollect;

    private void Start()
    {
        DungeonGenerator.Instance.OnDungeonComplete += Initialize;
        cinemachineInput.XYAxis.Set(InputManager.playerAction.Gameplay.Look);
    }

    private void Initialize()
    {
        timer = levelData.Duration;

        StartCoroutine(CountTimer());
    }

    private IEnumerator CountTimer()
    {
        while (true)
        {
            UpdateTimerUI();
            if (timer <= 0f)
            {
                End();
                GameTimerEnd?.Invoke();
                break;
            }
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }
    }

    public void End()
    {
        GameEnded = true;
        GameEnd?.Invoke();
        // Disable Input
        InputManager.playerAction.Disable();
        StopAllCoroutines();
        EndPanel.SetActive(true);
    }

    private void UpdateTimerUI()
    {
        ts = TimeSpan.FromSeconds(timer);
        TimerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
    }

    public void CoinCollected()
    {
        OnCoinCollect?.Invoke();
        AddCoins(levelData.CoinsPerCollect);
    }

    public void EnemyDrop()
    {
        AddCoins(levelData.CoinsPerEnemy);
    }

    private void AddCoins(int value)
    {
        levelData.Coins += value;
    }

    public void RestartLevel()
    {
        InputManager.playerAction.Enable();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #region GUI
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 40), "Coins : " + levelData.Coins.ToString());
    }
    #endregion
}
