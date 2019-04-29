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
    public GameObject explosionSound;
    public AudioClip monsterAggroSound;
    public AudioClip playerDeath;
    public AudioClip playerScream;
    public AudioClip itemPickup;

    public List<AudioClip> painSounds;
    public List<AudioClip> monsterPainSounds;

    List<GameObject> shotSoundInstances = new List<GameObject>();

    private AudioSource _audioSource;

    private static SoundManager instance = null;
    public static SoundManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            instance = FindObjectOfType<SoundManager>();
            if (instance == null || instance.Equals(null))
            {
                Debug.LogError("The scene needs a SoundManager");
            }
            instance._audioSource = instance.gameObject.GetComponent<AudioSource>();

            return instance;
        }
    }

    private void FixedUpdate()
    {
        for(int i = shotSoundInstances.Count - 1; i >= 0; i--)
        {
            if (shotSoundInstances[i] == null)
                shotSoundInstances.RemoveAt(i);
        }
    }

    public void PlayItemPickup()
    {
        _audioSource.PlayOneShot(itemPickup, SettingsManager.Instance.SFXVolume);
    }

    public GameObject PlayShotSound(bool loop)
    {
        if (shotSoundInstances.Count > 20)
            return null;

        GameObject go = GameObject.Instantiate(simpleShotSound);
        AudioSource audioSource = go.GetComponent<AudioSource>();
        audioSource.volume = SettingsManager.Instance.SFXVolume;

        Destroy(go, audioSource.clip.length);

        shotSoundInstances.Add(go);

        return go;
    }

    public void PlayMonsterAggro()
    {
        _audioSource.PlayOneShot(monsterAggroSound, SettingsManager.Instance.SFXVolume);
    }

    public GameObject PlayExplosionSound()
    {
        GameObject go = GameObject.Instantiate(explosionSound);
        AudioSource audioSource = go.GetComponent<AudioSource>();
        audioSource.volume = SettingsManager.Instance.SFXVolume;

        Destroy(go, audioSource.clip.length);
        return go;
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
            _audioSource.PlayOneShot(playerScream, SettingsManager.Instance.SFXVolume);
        else
            _audioSource.PlayOneShot(playerDeath, SettingsManager.Instance.SFXVolume);
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
