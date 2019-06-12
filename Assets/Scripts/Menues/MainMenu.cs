using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : Menu
{
    bool initialized = false;
    public override void OnEnter()
    {
        Cursor.visible = true;
        if (!initialized)
        {
            MusicController.Instance.PlayMusic("MainMenu");
            initialized = true;
        }
    }

    public override void OnPressedEscape()
    {
        OnQuitClick();
    }

    public void OnCreditsClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PushMenu<CreditsMenu>();
    }

    public void OnInstructionsClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PushMenu<InstructionsMenu>();
    }

    public void OnOptionsClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PushMenu<OptionsMenu>();
    }

    public void OnStartClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PopMenu();
        GameSceneManager.Instance.LoadHubScene();
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
