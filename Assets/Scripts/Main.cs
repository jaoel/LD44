using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Hubworld,
    Shop,
    Graveyard,
    Gameplay,
    Count
}

public class Main : MonoBehaviour
{
    public GameState gameState;
    public Player player;
    
    public bool gameOver;
    public GameObject blackOverlay;

    public bool Paused { get; private set; } = false;

    private static Main _instance = null;
    public static Main Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<Main>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a Main");
            }
            return _instance;
        }
    }

    void Awake()
    {
        DOTween.Init();
    }

    private void Start()
    {
        Time.timeScale = 1.0f;
    }

    void Update()
    {
        if (MenuManager.Instance.IsOpen())
        {
            blackOverlay.SetActive(true);
            return;
        }

        if (!player.IsAlive)
        {
            MenuManager.Instance.PushMenu<GameOverMenu>();
        }

        Paused = false;
        blackOverlay.SetActive(false);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Paused = true;
            SoundManager.Instance.PlayUIButtonClick();
            MenuManager.Instance.PushMenu<PauseMenu>();
        }
    }

    public void LoadDungeon()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void LoadHubworld()
    {
        SceneManager.LoadScene("HubworldScene", LoadSceneMode.Single);
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }
}
