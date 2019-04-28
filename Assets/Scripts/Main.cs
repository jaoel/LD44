using Assets.Scripts;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    public ShopRoom shopRoomPrefab;
    public TileContainer tileContainer;
    public InteractiveDungeonObject interactiveDungeonObjectContainer;
    public ItemContainer itemContainer;
    public EnemyContainer enemyContainer;
    public Player player;

    MapGenerator _mapGen;
    Map _currentMap;
    bool _renderBSP;

    public GameObject gameOverUI;
    public GameObject pauseUI;
    private bool _gamePaused;
    private ShopRoom shopInstance;

    private static Main instance = null;
    public static Main Instance {
        get {
            if (instance != null) {
                return instance;
            }
            instance = FindObjectOfType<Main>();
            if (instance == null || instance.Equals(null)) {
                Debug.LogError("The scene needs a Main");
            }
            return instance;
        }
    }

    void Start()
    {
        DOTween.Init();

        shopInstance = Instantiate(shopRoomPrefab);
        Time.timeScale = 1.0f;
        _gamePaused = false;
        _mapGen = new MapGenerator(tileContainer, interactiveDungeonObjectContainer, itemContainer,
            enemyContainer);
        _renderBSP = false;

        LoadLevel();
    }

    public void LoadLevel()
    {
        player.ResetPlayer();
        GenerateMap();
    }

    public void GenerateMap()
    {
        shopInstance.ClearItems();
        shopInstance.gameObject.SetActive(false);

        _currentMap = _mapGen.GenerateDungeon(Random.Range(5, 10), 100, 100);
        _currentMap.MovePlayerToSpawn(player);
        NavigationManager.map = _currentMap;
    }

    public void GenerateShop() {
        _mapGen.Clear();

        shopInstance.gameObject.SetActive(true);
        shopInstance.GenerateRandomItems();
        shopInstance.MovePlayerToSpawn(player);
    }

    public void DamageAllEnemiesInCircle(Vector2 position, float radius, int damage, bool damagePlayer) {
        List<Enemy> enemies = _mapGen.GetEnemiesInCircle(position, radius);
        foreach(Enemy enemy in enemies) {
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
        if (_currentMap != null && _renderBSP)
            _currentMap.DrawDebug();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause(!_gamePaused);
        }

        if (!player.IsAlive)
        {
            gameOverUI.SetActive(true);
        }
    }

    private void TogglePause(bool pause)
    {
        _gamePaused = pause;
        Time.timeScale = _gamePaused ? 0.0f : 1.0f;
        pauseUI.SetActive(_gamePaused);
    }

    public void OnClickStartGame()
    {
        TogglePause(false);
    }

    public void OnClickRestart()
    {
        TogglePause(false);
        gameOverUI.SetActive(false);
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void OnClickReturnToMainMenu()
    {
        TogglePause(false);
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }
}
