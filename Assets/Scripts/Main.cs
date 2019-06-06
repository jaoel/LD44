using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public ShopRoom shopRoomPrefab;
    public Player player;

    private Map _currentMap;
    
    public bool gameOver;
    private ShopRoom shopInstance;
    public GameObject blackOverlay;

    public int CurrentLevel;
    public Map CurrentMap => _currentMap;
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

        shopInstance = Instantiate(shopRoomPrefab);
        Time.timeScale = 1.0f;
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
        shopInstance.ClearItems();
        shopInstance.gameObject.SetActive(false);

        MapGeneratorParameters parameters = new MapGeneratorParameters();
        parameters.GenerationRadius = 20;

        parameters.MinCellSize = 4;
        parameters.MaxCellSize = 20;

        parameters.MinCellCount = 75;
        parameters.MaxCellCount = 150;

        parameters.RoomThresholdMultiplier = 1.25f;
        parameters.CorridorRoomConnectionFactor = 0.5f;
        parameters.MazeFactor = 0.15f;

        parameters.MinCorridorWidth = 4;
        parameters.MaxCorridorWidth = 5;

        parameters.MinRoomDistance = 0;
        parameters.LockFactor = 0.2f;

        _currentMap = MapGenerator.Instance.GenerateMap(DateTime.Now.Ticks, parameters, CurrentLevel);

        MapGenerator.Instance.PopulateMap(ref _currentMap, ref player, parameters, CurrentLevel);
        _currentMap.ActivateObjects();
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
            enemy.ReceiveDamage(damage, dir);
        }

        if (damagePlayer && Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.y), position) <= radius)
        {
            Vector2 dir = new Vector2(player.transform.position.x, player.transform.position.y) - position;
            player.ReceiveDamage(damage, dir);
        }
    }

    private void OnDrawGizmos()
    {
        if (_currentMap != null)
        {
            _currentMap.DrawDebug();
        }
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

    public void RestartGame()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }
}
