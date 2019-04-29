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
    public AudioClip shopMusic;
    public AudioClip deathJingle;
    public List<AudioClip> gameMusic;

    public AudioSource _audioSource;
    public AudioSource shopMusicSource;

    bool _fadingOutMusic = false;
    bool _playingGameplayMusic;

    AudioClip _lastClip;
    float _lastTime;

    Coroutine _running = null;

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
                return null;
            }

            DontDestroyOnLoad(instance.gameObject);
            return instance;
        }
    }

    private void FixedUpdate()
    {
        if (_playingGameplayMusic && _audioSource.clip?.length - _audioSource?.time < 1.0f && !_fadingOutMusic)
        {
            StartCoroutine(PlayMusic("RandomGameplay", false, 1.0f));
        }
    }

    public IEnumerator PlayMusic(string key, bool loop = true, float fadeTime = 0.2f)
    {
        while (_running != null)
            yield return new WaitForSeconds(0.01f);   

        _playingGameplayMusic = false;      
        if (key == "MainMenu")
        {
            _running = StartCoroutine(PlayMusicFade(_audioSource, menuMusic, loop, fadeTime));
        }
        else if (key == "RandomGameplay")
        {
            _running = StartCoroutine(PlayMusicFade(_audioSource, gameMusic[UnityEngine.Random.Range(0, gameMusic.Count)], 
                loop, fadeTime));
            _playingGameplayMusic = true;
        }
        else if (key == "Defeat")
        {
            _running = StartCoroutine(PlayMusicFade(_audioSource, deathJingle, loop, fadeTime));
        }
        else if (key == "Shop")
        {                                                                                        
            _lastClip = _audioSource.clip;
            _lastTime = _audioSource.time;

            _running = StartCoroutine(PlayMusicFade(_audioSource, shopMusic, loop, fadeTime));
        }
        else if (key == "ResumeGameplay")
        {
            _playingGameplayMusic = true;
            _running = StartCoroutine(PlayMusicFade(_audioSource, _lastClip, loop, fadeTime, _lastTime));
        }
    }

    private IEnumerator PlayMusicFade(AudioSource source, AudioClip clip, bool loop, float fadeTime, float time = 0.0f)
    {
        while (_fadingOutMusic)
        {
            yield return new WaitForSeconds(0.01f);
        }

        float fadeHalfTime = fadeTime / 2.0f;

        if (source.isPlaying)
        {
            StartCoroutine(FadeOut(source, fadeHalfTime));

            yield return new WaitForSeconds(fadeHalfTime);
        }

        source.clip = clip;
        source.loop = loop;
        source.volume = 0.1f;
        source.time = time;
        source.Play();

        StartCoroutine(FadeIn(source, fadeHalfTime));
        yield break;
    }

    private IEnumerator FadeOut(AudioSource audioSource,float fadeTime)
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
        _running = null;
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
