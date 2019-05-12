public class CreditsMenu : Menu
{
    public void OnMainMenuClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PopMenu();
    }

    public override void OnPressedEscape()
    {
        OnMainMenuClick();
    }
}
