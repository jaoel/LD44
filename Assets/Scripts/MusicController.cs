using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioClip menuMusic;
    public AudioClip deathJingle;
    public List<AudioClip> gameMusic;

    private AudioSource _audioSource;
    bool _fadingOutMusic = false;
    bool _playingGameplayMusic;

    private static MusicController instance = null;
    public static MusicController Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            instance = FindObjectOfType<MusicController>();
            if (instance == null || instance.Equals(null))
            {
                Debug.LogError("The scene needs a MusicController");
            }
            DontDestroyOnLoad(instance.gameObject);
            instance._audioSource = instance.gameObject.GetComponent<AudioSource>();
            return instance;
        }
    }

    public void PlayMenuMusic()
    {
        _audioSource.clip = menuMusic;
        _audioSource.Play();
    }

    private void FixedUpdate()
    {
        if (_playingGameplayMusic && _audioSource.clip.length - _audioSource.time < 1.0f && !_fadingOutMusic)
        {
            PlayMusic("RandomGameplay", false, 1.0f);
        }
    }

    public void PlayMusic(string key, bool loop = true, float fadeTime = 0.2f)
    {
        _playingGameplayMusic = false;

        if (key == "MainMenu")
        {
            StartCoroutine(PlayMusicFade(menuMusic, loop, fadeTime));
        }
        else if (key == "RandomGameplay")
        {
            StartCoroutine(PlayMusicFade(gameMusic[UnityEngine.Random.Range(0, gameMusic.Count)], loop, fadeTime));
            _playingGameplayMusic = true;
        }
        else if (key == "Defeat")
        {
            StartCoroutine(PlayMusicFade(deathJingle, loop, fadeTime));
        }
    }

    private IEnumerator PlayMusicFade(AudioClip clip, bool loop, float fadeTime)
    {
        while (_fadingOutMusic)
            yield return new WaitForSeconds(0.01f);

        float fadeHalfTime = fadeTime / 2.0f;

        if (_audioSource.isPlaying)
        {
            StartCoroutine(FadeOut(_audioSource, fadeHalfTime));

            yield return new WaitForSeconds(fadeHalfTime);
        }

        _audioSource.clip = clip;
        _audioSource.loop = loop;
        _audioSource.volume = 0.1f;
        _audioSource.Play();

        StartCoroutine(FadeIn(_audioSource, fadeHalfTime));
        yield break;
    }

    private IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
    {
        _fadingOutMusic = true;
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
        _fadingOutMusic = false;
    }

    private IEnumerator FadeIn(AudioSource audioSource, float fadeTime)
    {
        float startVolume = Math.Min(0.1f, 0.2f);

        audioSource.volume = 0;
        audioSource.Play();

        while (audioSource.volume < 0.2f)
        {
            audioSource.volume += startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        audioSource.volume = 0.2f;
    }
}
