using UnityEngine.UI;

public class OptionsMenu : Menu
{
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    public override void OnEnter()
    {
        base.OnEnter();
        sfxVolumeSlider.value = SettingsManager.Instance.LinearSFXVolume;
        musicVolumeSlider.value = SettingsManager.Instance.LinearMusicVolume;
    }

    public override void OnPressedEscape()
    {
        OnBackClick();
    }

    public void OnBackClick()
    {
        SoundManager.Instance.PlayUIButtonClick();
        MenuManager.Instance.PopMenu();
    }

    public void OnMusicSliderValueChange(float value)
    {
        SettingsManager.Instance.LinearMusicVolume = value;

        if (MusicController.Instance != null)
        {
            MusicController.Instance.SetVolume();
        }
    }

    public void OnSfxSliderValueChange(float value)
    {
        SettingsManager.Instance.LinearSFXVolume = value;

        if (MusicController.Instance != null)
        {
            MusicController.Instance.SetVolume();
        }
    }
}
