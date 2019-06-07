using UnityEngine;

public static class PlayerPrefsStrings
{
    public static readonly string MusicVolume = "settings_music_volume";
    public static readonly string SfxVolume = "settings_sfx_volume";
    public static readonly string ScreenShakeScale = "settings_screen_shake_scale";
    public static readonly string ShowDebugMenu = "settings_show_debug_menu";
}

public class SettingsManager
{
    private static SettingsManager _instance;

    private float _linearMusicVolume = 0f;
    private float _linearSFXVolume = 0f;
    private float _screenShakeScale;

    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }

    public float ScreenShakeScale
    {
        get { return _screenShakeScale; }
        set
        {
            _screenShakeScale = value;
            PlayerPrefs.SetFloat(PlayerPrefsStrings.ScreenShakeScale, value);
        }
    }

    public float LinearMusicVolume
    {
        get { return _linearMusicVolume; }
        set
        {
            _linearMusicVolume = value;
            MusicVolume = value * value;
            PlayerPrefs.SetFloat(PlayerPrefsStrings.MusicVolume, value);
        }
    }

    public float LinearSFXVolume
    {
        get { return _linearSFXVolume; }
        set
        {
            _linearSFXVolume = value;
            SFXVolume = value * value;
            PlayerPrefs.SetFloat(PlayerPrefsStrings.SfxVolume, value);
        }
    }

    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SettingsManager();
                _instance.LinearMusicVolume = PlayerPrefs.GetFloat(PlayerPrefsStrings.MusicVolume, 0.5f);
                _instance.LinearSFXVolume = PlayerPrefs.GetFloat(PlayerPrefsStrings.SfxVolume, 0.5f);
                _instance.ScreenShakeScale = PlayerPrefs.GetFloat(PlayerPrefsStrings.ScreenShakeScale, 1.0f);
            }

            return _instance;
        }
    }

    private SettingsManager()
    {
        SFXVolume = 0.2f;
        MusicVolume = 0.2f;
    }
}
