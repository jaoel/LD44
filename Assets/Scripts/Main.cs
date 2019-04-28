using Assets.Scripts;
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
            Time.timeScale = 0.0f;
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
