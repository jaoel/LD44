using Assets.Scripts;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public Map _currentMap;
    bool _renderBSP;

    public GameObject gameOverUI;
    public GameObject pauseUI;
    public GameObject optionsMenu;
    public bool _gamePaused;
    public bool _gameOver;
    private ShopRoom shopInstance;
    public TextMeshProUGUI currentLevelText;

    private int _currentLevel;
    public int CurrentLevel => _currentLevel;

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

    void Awake()
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

    public void DebugDrawPath(List<Vector2Int> path)
    {
        _currentMap.DrawPath(path);
    }

    public void LoadLevel()
    {          
        player.ResetPlayer();
        GenerateMap();
    }

    public void GenerateMap()
    {
        Cursor.visible = false;

        if (MusicController.Instance != null)
        {
            if (_currentLevel == 0)
            {
                MusicController.Instance.PlayMusic("RandomGameplay", false);
            }
            else
            {
                MusicController.Instance.PlayMusic("ResumeGameplay", false, 1.0f);
            }
        }  

        _currentLevel++;
        currentLevelText.text = "Level " + _currentLevel;
        shopInstance.ClearItems();
        shopInstance.gameObject.SetActive(false);

        _currentMap = _mapGen.GenerateDungeon(Random.Range(5, 10), Random.Range(30,60), Random.Range(30, 60), _currentLevel, player);
        NavigationManager.map = _currentMap;
    }

    public void GenerateShop() {
        _mapGen.Clear();

        if (MusicController.Instance != null)
            MusicController.Instance.PlayMusic("Shop");

        shopInstance.gameObject.SetActive(true);
        shopInstance.GenerateRandomItems(_currentLevel);
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

        if (!player.IsAlive && !_gameOver)
        {
            Cursor.visible = true;
            _gameOver = true;

            if (MusicController.Instance != null)
                MusicController.Instance.PlayMusic("Defeat", false);

            gameOverUI.SetActive(true);
            currentLevelText.gameObject.SetActive(false);
            GameObject.Find("GameOverLevel").GetComponent<TextMeshProUGUI>().text = "You reached level " + _currentLevel; 
        }
    }

    private void TogglePause(bool pause)
    {
        Cursor.visible = pause;

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
        Cursor.visible = true;
        TogglePause(false);
        gameOverUI.SetActive(false);
                                                                       
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void OnClickReturnToMainMenu()
    {
        Cursor.visible = true;
        TogglePause(false);
        gameOverUI.SetActive(false);
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }

    public void OnOptionsClick()
    {
        Cursor.visible = true;
        optionsMenu.SetActive(true);
        pauseUI.SetActive(false);
    }
}
