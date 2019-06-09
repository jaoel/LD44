using System;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour 
{
    public Map CurrentMap => _currentMap;

    public int CurrentLevel;

    [SerializeField]
    private ShopRoom _shopRoomPrefab;

    [SerializeField]
    private Player _player;

    private ShopRoom _shopInstance;
    private Map _currentMap;
    private bool _fogOfWarVisible = true;
    private bool _fogOfWarEnabled = true;

    private static MapManager _instance = null;
    public static MapManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<MapManager>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a MapManager");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        
    }

    private void Start()
    {
        _shopInstance = Instantiate(_shopRoomPrefab);
        LoadLevel();
    }

    private void OnDrawGizmos()
    {
        if (_currentMap != null)
        {
            _currentMap.DrawDebug();
        }
    }

    private void LoadLevel()
    {
        _player.ResetPlayer();
        GenerateMap();
    }

    public void ToggleFogOfWarEnabled(bool toggle)
    {
        _fogOfWarEnabled = toggle;
        ShowFogOfWar(_fogOfWarEnabled);
    }

    private void GenerateFogOfWar()
    {
        FogOfWar fogOfWar = GetComponent<FogOfWar>();
        if (fogOfWar)
        {
            fogOfWar.GenerateTexture();
        }
    }

    private void ShowFogOfWar(bool show)
    {
        FogOfWar fogOfWar = GetComponent<FogOfWar>();
        if (fogOfWar)
        {
            fogOfWar.SetEnabled(show ? (_fogOfWarEnabled && _fogOfWarVisible) : false);
        }
    }

    public void GenerateMap()
    {
        GenerateMapWithSeed(DateTime.Now.Ticks);
    }

    public void GenerateMapWithSeed(long seed)
    {
        if (_currentMap != null)
        {
            _currentMap.ClearMap();
        }
        BulletManager.Instance.Clear();
        Cursor.visible = false;

        if (MusicController.Instance != null)
        {
            if (CurrentLevel == 0)
            {
                MusicController.Instance.PlayMusic("RandomGameplay", false);
            }
            else
            {
                MusicController.Instance.PlayMusic("ResumeGameplay", false, 1.0f);
            }
        }

        CurrentLevel++;
        _shopInstance.ClearItems();
        _shopInstance.gameObject.SetActive(false);

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

        _currentMap = MapGenerator.Instance.GenerateMap(seed, parameters, CurrentLevel);

        MapGenerator.Instance.PopulateMap(ref _currentMap, ref _player, parameters, CurrentLevel);
        _currentMap.ActivateObjects();

        GenerateFogOfWar();
        _fogOfWarVisible = true;
        ShowFogOfWar(_fogOfWarEnabled);
    }

    public void GenerateShop()
    {
        BulletManager.Instance.Clear();
        _currentMap.ClearMap();

        if (MusicController.Instance != null)
        {
            MusicController.Instance.PlayMusic("Shop");
        }

        _fogOfWarVisible = false;
        ShowFogOfWar(false);

        _shopInstance.gameObject.SetActive(true);
        _shopInstance.GenerateRandomItems(CurrentLevel, _player);
        _shopInstance.MovePlayerToSpawn(_player);
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

        if (damagePlayer && Vector2.Distance(
            new Vector2(_player.transform.position.x, _player.transform.position.y), position) <= radius)
        {
            Vector2 dir = new Vector2(_player.transform.position.x, _player.transform.position.y) - position;
            _player.ReceiveDamage(damage / 10, dir);
        }
    }
}
