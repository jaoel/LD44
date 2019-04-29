using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public GameObject previousMenu;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void OnEnable()
    {
        sfxVolumeSlider.value = SettingsManager.Instance.SFXVolume;
        musicVolumeSlider.value = SettingsManager.Instance.MusicVolume;
    }

    public void OnMainMenuClick()
    {
        SettingsManager.Instance.SFXVolume = sfxVolumeSlider.value;
        SettingsManager.Instance.MusicVolume = musicVolumeSlider.value;

        MusicController.Instance.SetVolume();

        previousMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
