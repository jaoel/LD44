using UnityEngine;

public class PauseMenu : Menu
{
    [SerializeField]
    private GameObject _restart;

    [SerializeField]
    private GameObject _hubworld;

    [SerializeField]
    private RectTransform _space;

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
            _restart.SetActive(false);
            _hubworld.SetActive(false);
            _space.sizeDelta = new Vector2(_space.sizeDelta.x, 48);
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
