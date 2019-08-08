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
    Boss,
    Count
}

public class Main : MonoBehaviour
{
    public GameSessionData sessionData = new GameSessionData();
    public GameState gameState;
    public Player player;
    
    public bool gameOver;
    public GameObject blackOverlay;
    public List<int> bossTiers;

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
        bossTiers = new List<int>();
    }

    public void Initialize()
    {
        gameOver = false;
        Time.timeScale = 1.0f;

        if (gameState != GameState.Boss)
        {
            bossTiers = new List<int>();
        }
    }

    public void PayBossTribute(int tier)
    {
        bossTiers.Add(tier);
        GameObject.Find("BossTeleport").GetComponent<BossTeleport>().active = true;
    }

    void Update()
    {
        if (MenuManager.Instance.IsOpen())
        {
            blackOverlay.SetActive(true);
            return;
        }

        if (!player.IsAlive && gameState != GameState.MainMenu && GameSceneManager.Instance.LoadingState == LoadingState.NOT_LOADING)
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
}
