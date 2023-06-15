using Cinemachine;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public GameObject PausePanel;
    public GameObject EndPanel;
    public TMP_Text CoinsUILabel;
    public CinemachineInputProvider cinemachineInput;
    public DungeonNavMesh DungeonNavMesh;

    public Action GameTimerEnd;
    public Action GameEnd;
    public Action PlayerWin;
    public Action PlayerLose;

    [Header("Level")]
    [SerializeField] private LevelData levelData;
    private float timer;
    private TimeSpan ts;

    public int coinsValue => levelData.CoinsPerCollect;
    public Action OnCoinCollect;

    private void Start()
    {
        DungeonGenerator.Instance.OnDungeonComplete += Initialize;
        cinemachineInput.XYAxis.Set(InputManager.playerAction.Gameplay.Look);

        InputManager.playerAction.Gameplay.Pause.performed += PauseGame;
    }

    private void Initialize()
    {
        timer = levelData.Duration;

        StartCoroutine(CountTimer());
    }

    private void Update()
    {
        CoinsUILabel.text = levelData.Coins.ToString();
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
        InputManager.ToggleActionMap(InputManager.playerAction.Panel);
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


    public bool DeductCoins(int value)
    {
        if (value > 0 && levelData.Coins >= value)
        {
            levelData.Coins -= value;
            return true;
        }
        return false;
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        Time.timeScale = 0;
        PausePanel.SetActive(true);
        InputManager.ToggleActionMap(InputManager.playerAction.Panel);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        PausePanel.SetActive(false);
        InputManager.ToggleActionMap(InputManager.playerAction.Gameplay);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1;
        InputManager.ToggleActionMap(InputManager.playerAction.Gameplay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #region GUI
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 40), "Coins : " + levelData.Coins.ToString());
    }
    #endregion
}
