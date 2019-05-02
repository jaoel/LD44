using UnityEngine;

public class SettingsManager {
    private float linearMusicVolume = 0f;
    private float linearSFXVolume = 0f;

    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }
    public float ScreenShakeScale { get; set; }

    public float LinearMusicVolume {
        get {
            return linearMusicVolume;
        }
        set {
            linearMusicVolume = value;
            MusicVolume = Mathf.Pow(value, 4f);
        }
    }

    public float LinearSFXVolume {
        get {
            return linearSFXVolume;
        }
        set {
            linearSFXVolume = value;
            SFXVolume = Mathf.Pow(value, 4f);
        }
    }

    private static SettingsManager _instance;

    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SettingsManager();
                _instance.LinearMusicVolume = 0.5f;
                _instance.LinearSFXVolume = 0.5f;
                _instance.ScreenShakeScale = 1.0f;
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
