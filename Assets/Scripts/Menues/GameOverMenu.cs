using TMPro;
using UnityEngine;

public class GameOverMenu : Menu
{
    public TextMeshProUGUI gameOverLevelText;

    public override void OnEnter()
    {
        base.OnEnter();
        gameOverLevelText.text = Main.Instance.CurrentLevel.ToString();
        Cursor.visible = true;
        MusicController.Instance.PlayMusic("Defeat", false);
    }

    public override void OnExit()
    {
        base.OnExit();
        Cursor.visible = false;
    }

    public void OnMainMenuClick()
    {
        Main.Instance.QuitToMenu();
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PopMenu();
    }

    public override void OnPressedEscape()
    {
    }

    public void OnRestartClick()
    {
        Main.Instance.RestartGame();
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PopMenu();
    }
}
