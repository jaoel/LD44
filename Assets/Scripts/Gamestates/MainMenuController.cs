using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject optionsMenu;
    public GameObject instructionsMenu;
    public GameObject creditsMenu;

    public void Awake()
    {
        Cursor.visible = true;
        MusicController.Instance.PlayMusic("MainMenu");
    }

    public void OnCreditsClick()
    {
        creditsMenu.SetActive(!creditsMenu.activeInHierarchy);
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    public void OnInstructionsClick()
    {
        instructionsMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnOptionsClick()
    {
        optionsMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnStartClick()
    {
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
