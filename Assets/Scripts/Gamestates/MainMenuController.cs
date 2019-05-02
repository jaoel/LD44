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
        SoundManager.Instance.PlayUIButtonClick();
    }

    public void OnInstructionsClick()
    {
        instructionsMenu.SetActive(true);
        gameObject.SetActive(false);
        SoundManager.Instance.PlayUIButtonClick();
    }

    public void OnOptionsClick()
    {
        optionsMenu.SetActive(true);
        gameObject.SetActive(false);
        SoundManager.Instance.PlayUIButtonClick();
    }

    public void OnStartClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }


    public void OnQuitClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif     
    }
}
