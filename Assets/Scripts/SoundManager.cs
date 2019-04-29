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
        _audioSource.PlayOneShot(itemPickup);
    }

    public GameObject PlayShotSound(bool loop)
    {
        if (shotSoundInstances.Count > 20)
            return null;

        GameObject go = GameObject.Instantiate(simpleShotSound);
        Destroy(go, go.GetComponent<AudioSource>().clip.length);

        shotSoundInstances.Add(go);

        return go;
    }

    public void PlayMonsterAggro()
    {
        _audioSource.PlayOneShot(monsterAggroSound);
    }

    public GameObject PlayExplosionSound()
    {
        GameObject go = GameObject.Instantiate(explosionSound);
        Destroy(go, go.GetComponent<AudioSource>().clip.length);
        return go;
    }

    public GameObject PlayMachinegun()
    {
        GameObject go = GameObject.Instantiate(machineGunSound);
        Destroy(go, go.GetComponent<AudioSource>().clip.length);

        return go;
    }

    public void PlayPainSound()
    {
        AudioClip temp = painSounds[UnityEngine.Random.Range(0, painSounds.Count)];
        _audioSource.PlayOneShot(temp);
    }

    public void PlayMonsterPainSound()
    {
        AudioClip temp = monsterPainSounds[UnityEngine.Random.Range(0, monsterPainSounds.Count)];
        _audioSource.PlayOneShot(temp);
    }

    public void PlayPlayerDeath(bool memes)
    {
        if (memes)
            _audioSource.PlayOneShot(playerScream);
        else
            _audioSource.PlayOneShot(playerDeath);
    }

    public AudioSource PlaySound(GameObject prefab, bool loop)
    {
       
        if (loop)
        {
            GameObject go = GameObject.Instantiate(prefab);
            AudioSource audioSource = go.GetComponent<AudioSource>();
            audioSource.loop = true;
            return audioSource;
        }
        else
        {
            _audioSource.PlayOneShot(prefab.GetComponent<AudioSource>().clip);
            return null;
        }  
    }   
}
