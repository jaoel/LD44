﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void Awake()
    {
        MusicController.Instance.PlayMusic("MainMenu");
    }

    public void OnStartClick()
    {
        MusicController.Instance.PlayMusic("RandomGameplay", false);
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif     
    }
}
