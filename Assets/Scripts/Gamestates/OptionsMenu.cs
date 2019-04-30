using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public GameObject previousMenu;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void OnEnable() {
        sfxVolumeSlider.value = SettingsManager.Instance.LinearSFXVolume;
        musicVolumeSlider.value = SettingsManager.Instance.LinearMusicVolume;
    }

    public void OnMainMenuClick() {
        previousMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnMusicSliderValueChange(float value) {
        SettingsManager.Instance.LinearMusicVolume = value;

        if (MusicController.Instance != null) {
            MusicController.Instance.SetVolume();
        }
    }

    public void OnSfxSliderValueChange(float value) {
        SettingsManager.Instance.LinearSFXVolume = value;

        if (MusicController.Instance != null) {
            MusicController.Instance.SetVolume();
        }
    }
}
