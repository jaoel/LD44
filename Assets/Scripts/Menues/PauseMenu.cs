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

    public void OnMainMenuClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        Main.Instance.QuitToMenu();
        MenuManager.Instance.PopMenu();
    }
}
