using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SettingsManager
{
    public float MusicVolume { get; set; }
    public float SFXVolume { get; set; }

    private static SettingsManager _instance;

    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SettingsManager();
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
