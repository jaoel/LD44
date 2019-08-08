using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public LoadingState LoadingState => _loadingScreen.LoadingState;

    [SerializeField]
    private GameObject _managerContainer;

    [SerializeField]
    private GameObject _playerContainer;

    [SerializeField]
    private LoadingScreen _loadingScreen;

    private bool _instantiated = false;
    private static GameSceneManager _instance = null;
    public static GameSceneManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<GameSceneManager>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a GameSceneManager");
            }

            DontDestroyOnLoad(_instance.gameObject);

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null && _instantiated == false)
        {
            _instantiated = true;
            GameSceneManager temp = GameSceneManager.Instance;
        }
        else if (_instance != null && _instantiated == false)
        {
            DestroyImmediate(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Main.Instance.sessionData.playerMaxHealth == 0)
        {
            Main.Instance.sessionData.LoadData();
        }

        MapManager.Instance.Reset();

        MapGenerator.Instance.Initialize();
        MapManager.Instance.Initialize();
        Main.Instance.Initialize();
    }

    public void ToggleManagerContainer(bool active)
    {
        _managerContainer.SetActive(active);
    }

    public void TogglePlayerContainer(bool active)
    {
        _playerContainer.SetActive(active);
    }

    public void FadeScreen(Func<AsyncOperation> onLoad = null, float fadeTime = 1.0f)
    {
        _loadingScreen.StartLoad(onLoad, fadeTime);
    }

    public void LoadGameplayScene()
    {
        Func<AsyncOperation> onLoad = () =>
        {
            GameSceneManager.Instance.TogglePlayerContainer(true);
            Main.Instance.player.ResetPlayer();
            Main.Instance.gameState = GameState.Gameplay;
            return SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);
        };

        _loadingScreen.StartLoad(onLoad);
    }

    public void LoadHubScene()
    {
        Func<AsyncOperation> onLoad = () =>
        {
            GameSceneManager.Instance.TogglePlayerContainer(true);
            Main.Instance.gameState = GameState.Hubworld;
            Main.Instance.player.transform.position = Vector3.zero;
            Main.Instance.player.ResetPlayer();
            MapManager.Instance.ToggleFogOfWarEnabled(true);
            return SceneManager.LoadSceneAsync("HubworldScene", LoadSceneMode.Single);
        };

        _loadingScreen.StartLoad(onLoad);
    } 

    public void LoadMainMenuScene()
    {
        Func<AsyncOperation> onLoad = () =>
        {
            GameSceneManager.Instance.TogglePlayerContainer(false);
            MapManager.Instance.ToggleFogOfWarEnabled(false);
            Main.Instance.player.ResetPlayer();
            Main.Instance.player.ClearWeapons();
            Main.Instance.gameState = GameState.MainMenu;
            MenuManager.Instance.PushMenu<MainMenu>();
            return SceneManager.LoadSceneAsync("MainMenuScene", LoadSceneMode.Single);
        };

        _loadingScreen.StartLoad(onLoad);
    }

    public void LoadBossScene()
    {
        Func<AsyncOperation> onLoad = () =>
        {
            GameSceneManager.Instance.TogglePlayerContainer(true);
            Main.Instance.gameState = GameState.Boss;
            Main.Instance.player.transform.position = Vector3.zero;
            Main.Instance.player.ResetPlayer(false);
            MapManager.Instance.ToggleFogOfWarEnabled(true);
            return SceneManager.LoadSceneAsync(MapManager.Instance.selectedDungeonData.bossScene, LoadSceneMode.Single);
        };

        _loadingScreen.StartLoad(onLoad);
    }
}
