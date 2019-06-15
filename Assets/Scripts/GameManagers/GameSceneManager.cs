using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _managerContainer;

    [SerializeField]
    private GameObject _playerContainer;

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

    public void LoadGameplayScene()
    {
        GameSceneManager.Instance.TogglePlayerContainer(true);
        Main.Instance.player.ResetPlayer();
        Main.Instance.gameState = GameState.Gameplay;
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void LoadHubScene()
    {
        GameSceneManager.Instance.TogglePlayerContainer(true);
        Main.Instance.gameState = GameState.Hubworld;
        Main.Instance.player.transform.position = Vector3.zero;
        Main.Instance.player.ResetPlayer();
        MapManager.Instance.ToggleFogOfWarEnabled(false);
        SceneManager.LoadScene("HubworldScene", LoadSceneMode.Single);
    } 

    public void LoadMainMenuScene()
    {
        GameSceneManager.Instance.TogglePlayerContainer(false);
        MapManager.Instance.ToggleFogOfWarEnabled(false);
        Main.Instance.player.ResetPlayer();
        Main.Instance.player.ClearWeapons();
        Main.Instance.gameState = GameState.MainMenu;
        MenuManager.Instance.PushMenu<MainMenu>();
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }
}
