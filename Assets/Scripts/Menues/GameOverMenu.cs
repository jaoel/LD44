using TMPro;
using UnityEngine;

public class GameOverMenu : Menu
{
    public TextMeshProUGUI gameOverLevelText;

    public override void OnEnter()
    {
        base.OnEnter();
        gameOverLevelText.text = MapManager.Instance.CurrentLevel.ToString();
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
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PopMenu();
        GameSceneManager.Instance.LoadMainMenuScene();
    }

    public override void OnPressedEscape()
    {
    }

    public void OnRestartClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PopMenu();

        switch (Main.Instance.gameState)
        {
            case GameState.Hubworld:
                GameSceneManager.Instance.LoadHubScene();
                break;
            case GameState.Gameplay:
                GameSceneManager.Instance.LoadGameplayScene();
                break;
        }
    }
}
