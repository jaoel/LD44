using UnityEngine;

public class PauseMenu : Menu
{
    public override void OnPressedEscape()
    {
        OnContinueClick();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Cursor.visible = true;
        Time.timeScale = 0f;

        if (Main.Instance.gameState != GameState.Gameplay)
        {
            GameObject.Find("Restart").SetActive(false);
            GameObject.Find("Hubworld").SetActive(false);

            RectTransform space = GameObject.Find("Space").GetComponent<RectTransform>();
            space.sizeDelta = new Vector2(space.sizeDelta.x, 48);
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    public void OnContinueClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PopMenu();
    }

    public void OnRestartClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        Main.Instance.RestartGame();
        MenuManager.Instance.PopMenu();
    }

    public void OnOptionsClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PushMenu<OptionsMenu>();
    }

    public void OnHubworldClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        Main.Instance.LoadHubworld();
        MenuManager.Instance.PopMenu();
    }

    public void OnMainMenuClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        Main.Instance.QuitToMenu();
        MenuManager.Instance.PopMenu();
    }
}
