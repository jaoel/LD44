using Assets.Scripts;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    public ShopRoom shopRoomPrefab;
    public Player player;

    private Map _currentMap;

    public GameObject gameOverUI;
    public GameObject pauseUI;
    public GameObject optionsMenu;
    public bool gamePaused;
    public bool gameOver;
    private ShopRoom shopInstance;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI gameOverLevelText;
    public GameObject blackOverlay;

    public int CurrentLevel { get; private set; } = 0;

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

        shopInstance = Instantiate(shopRoomPrefab);
        Time.timeScale = 1.0f;
        gamePaused = false;
        LoadLevel();
    }

    public void LoadLevel()
    {
        player.ResetPlayer();
        GenerateMap();
    }

    public void GenerateMap()
    {
        BulletManager.Instance.Clear();
        Cursor.visible = false;

        if (MusicController.Instance != null)
        {
            if (CurrentLevel == 0)
            {
                MusicController.Instance.PlayMusic("RandomGameplay", true);
            }
            else
            {
                MusicController.Instance.PlayMusic("ResumeGameplay", true, 1.0f);
            }
        }

        CurrentLevel++;
        currentLevelText.text = "Level " + CurrentLevel;
        shopInstance.ClearItems();
        shopInstance.gameObject.SetActive(false);


        MapGeneratorParameters parameters = new MapGeneratorParameters();
        parameters.GenerationRadius = 20;

        parameters.MinCellSize = 3;
        parameters.MaxCellSize = 30;

        parameters.MinCellCount = 20;
        parameters.MaxCellCount = 50;

        parameters.MinRoomWidth = 7;
        parameters.MinRoomHeight = 7;

        parameters.MinCorridorWidth = 3;
        parameters.MaxCorridorWidth = 5;

        _currentMap = MapGenerator.Instance.GenerateMap(DateTime.Now.Ticks, parameters);
        MapGenerator.Instance.PopulateMap(ref _currentMap, ref player, parameters);
        _currentMap.ActivateObjects();
        //NavigationManager.map = _currentBSPMap;
    }

    public void GenerateShop()
    {
        BulletManager.Instance.Clear();
        _currentMap.ClearMap();

        if (MusicController.Instance != null)
        {
            MusicController.Instance.PlayMusic("Shop");
        }

        shopInstance.gameObject.SetActive(true);
        shopInstance.GenerateRandomItems(CurrentLevel, player);
        shopInstance.MovePlayerToSpawn(player);
    }

    public void AddInteractiveObject(GameObject interactiveObject)
    {
        _currentMap.AddInteractiveObject(interactiveObject);
    }

    public void DamageAllEnemiesInCircle(Vector2 position, float radius, int damage, bool damagePlayer)
    {
        List<Enemy> enemies = _currentMap.GetEnemiesInCircle(position, radius);
        foreach (Enemy enemy in enemies)
        {
            Vector2 dir = new Vector2(enemy.transform.position.x, enemy.transform.position.y) - position;
            enemy.ApplyDamage(damage, dir);
        }

        if (damagePlayer && Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.y), position) <= radius)
        {
            Vector2 dir = new Vector2(player.transform.position.x, player.transform.position.y) - position;
            player.ReceiveDamage(damage, dir);
        }
    }

    private void OnDrawGizmos()
    {
    }

    void Update()
    {
        if (!gameOver && Input.GetKeyDown(KeyCode.Escape) && (!gamePaused || pauseUI.activeInHierarchy))
        {
            SoundManager.Instance.PlayUIButtonClick();
            TogglePause(!gamePaused);
        }

        if (!player.IsAlive && !gameOver)
        {
            TogglePause(false);
            Cursor.visible = true;
            gameOver = true;

            if (MusicController.Instance != null)
            {
                MusicController.Instance.PlayMusic("Defeat", false);
            }

            gameOverUI.SetActive(true);
            blackOverlay.SetActive(true);
            gameOverLevelText.text = CurrentLevel.ToString();
        }
    }

    private void TogglePause(bool pause)
    {
        Cursor.visible = pause;

        gamePaused = pause;
        Time.timeScale = gamePaused ? 0.0f : 1.0f;
        pauseUI.SetActive(gamePaused);
        blackOverlay.SetActive(gamePaused);
    }

    public void OnClickStartGame()
    {
        TogglePause(false);
        SoundManager.Instance.PlayUIButtonClick();
    }

    public void OnClickRestart()
    {
        Cursor.visible = true;
        TogglePause(false);
        gameOverUI.SetActive(false);
        blackOverlay.SetActive(false);
        SoundManager.Instance.PlayUIButtonClick();

        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void OnClickReturnToMainMenu()
    {
        Cursor.visible = true;
        TogglePause(false);
        gameOverUI.SetActive(false);
        blackOverlay.SetActive(false);
        SoundManager.Instance.PlayUIButtonClick();

        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }

    public void OnOptionsClick()
    {
        Cursor.visible = true;
        optionsMenu.SetActive(true);
        pauseUI.SetActive(false);
        SoundManager.Instance.PlayUIButtonClick();
    }
}
