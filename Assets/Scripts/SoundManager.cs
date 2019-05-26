using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public GameObject simpleShotSound;
    public GameObject machineGunSound;
    public AudioClip explosionSound;
    public AudioClip monsterAggroSound;
    public AudioClip playerDeath;
    public AudioClip playerScream;
    public AudioClip itemPickup;
    public AudioClip uiButtonClick;

    public List<AudioClip> painSounds;
    public List<AudioClip> monsterPainSounds;

    private List<GameObject> _shotSoundInstances = new List<GameObject>();

    private AudioSource _audioSource;

    private static SoundManager _instance = null;
    public static SoundManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<SoundManager>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a SoundManager");
            }

            _instance._audioSource = _instance.gameObject.GetComponent<AudioSource>();
            DontDestroyOnLoad(_instance.gameObject);

            return _instance;
        }
    }

    private void FixedUpdate()
    {
        for (int i = _shotSoundInstances.Count - 1; i >= 0; i--)
        {
            if (_shotSoundInstances[i] == null)
            {
                _shotSoundInstances.RemoveAt(i);
            }
        }
    }

    public void PlayItemPickup()
    {
        _audioSource.PlayOneShot(itemPickup, SettingsManager.Instance.SFXVolume);
    }

    public GameObject PlayShotSound(bool loop)
    {
        if (_shotSoundInstances.Count > 20)
        {
            return null;
        }

        GameObject go = GameObject.Instantiate(simpleShotSound);
        AudioSource audioSource = go.GetComponent<AudioSource>();
        audioSource.volume = SettingsManager.Instance.SFXVolume;

        Destroy(go, audioSource.clip.length);

        _shotSoundInstances.Add(go);

        return go;
    }

    public void PlayUIButtonClick()
    {
        _audioSource.PlayOneShot(uiButtonClick, SettingsManager.Instance.SFXVolume);
    }

    public void PlayMonsterAggro()
    {
        _audioSource.PlayOneShot(monsterAggroSound, SettingsManager.Instance.SFXVolume);
    }

    public void PlayExplosionSound()
    {
        _audioSource.PlayOneShot(explosionSound, SettingsManager.Instance.SFXVolume);
    }

    public GameObject PlayMachinegun()
    {
        GameObject go = GameObject.Instantiate(machineGunSound);
        AudioSource audioSource = go.GetComponent<AudioSource>();
        audioSource.volume = SettingsManager.Instance.SFXVolume;

        Destroy(go, audioSource.clip.length);
        return go;
    }

    public void PlayPainSound()
    {
        AudioClip temp = painSounds[UnityEngine.Random.Range(0, painSounds.Count)];
        _audioSource.PlayOneShot(temp, SettingsManager.Instance.SFXVolume);
    }

    public void PlayMonsterPainSound()
    {
        AudioClip temp = monsterPainSounds[UnityEngine.Random.Range(0, monsterPainSounds.Count)];
        _audioSource.PlayOneShot(temp, SettingsManager.Instance.SFXVolume);
    }

    public void PlayPlayerDeath(bool memes)
    {
        if (memes)
        {
            _audioSource.PlayOneShot(playerScream, SettingsManager.Instance.SFXVolume);
        }
        else
        {
            _audioSource.PlayOneShot(playerDeath, SettingsManager.Instance.SFXVolume);
        }
    }

    public AudioSource PlaySound(GameObject prefab, bool loop)
    {

        if (loop)
        {
            GameObject go = GameObject.Instantiate(prefab);
            AudioSource audioSource = go.GetComponent<AudioSource>();
            audioSource.volume = SettingsManager.Instance.SFXVolume;
            audioSource.loop = true;
            return audioSource;
        }
        else
        {
            _audioSource.PlayOneShot(prefab.GetComponent<AudioSource>().clip, SettingsManager.Instance.SFXVolume);
            return null;
        }
    }
}
