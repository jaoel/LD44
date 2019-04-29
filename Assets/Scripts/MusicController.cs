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

    bool _fadingOutMusic = false;
    bool _playingGameplayMusic;

    AudioClip _lastClip;
    float _lastTime;

    private Queue<float> _fadeTimers;
    private Queue<IEnumerator> _queuedCoroutines;

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

            instance._queuedCoroutines = new Queue<IEnumerator>();
            instance._fadeTimers = new Queue<float>();
            DontDestroyOnLoad(instance.gameObject);
            return instance;
        }
    }

    public void SetVolume()
    {
        _audioSource.volume = SettingsManager.Instance.MusicVolume;
    }

    private void Awake()
    {
        StartCoroutine(ProcessCoroutines());
    }

    private void Start() {
        SetVolume();
    }

    private IEnumerator ProcessCoroutines()
    {
        while(true)
        {
            if (_queuedCoroutines != null)
            {
                while (_queuedCoroutines.Count > 0)
                {
                    yield return StartCoroutine(_queuedCoroutines.Dequeue());
                    yield return new WaitForSeconds(_fadeTimers.Dequeue());
                }

            }
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        if (_playingGameplayMusic && _audioSource.clip?.length - _audioSource?.time < 1.0f)
        {
            PlayMusic("RandomGameplay", false, 1.0f);
            _playingGameplayMusic = false;
        }
    }

    public void PlayMusic(string key, bool loop = true, float fadeTime = 0.2f)
    {
        _fadeTimers.Enqueue(fadeTime);
        if (key == "MainMenu")
        {
            _queuedCoroutines.Enqueue(PlayMusicFade(_audioSource, menuMusic, loop, fadeTime));
        }
        else if (key == "RandomGameplay")
        {
            _queuedCoroutines.Enqueue(PlayMusicFade(_audioSource, gameMusic[UnityEngine.Random.Range(0, gameMusic.Count)],
                loop, fadeTime));
        }
        else if (key == "Defeat")
        {
            _queuedCoroutines.Enqueue(PlayMusicFade(_audioSource, deathJingle, loop, fadeTime));
        }
        else if (key == "Shop")
        {                                                                                        
            _lastClip = _audioSource.clip;
            _lastTime = _audioSource.time;

            _queuedCoroutines.Enqueue(PlayMusicFade(_audioSource, shopMusic, loop, fadeTime));
        }
        else if (key == "ResumeGameplay")
        {
            _playingGameplayMusic = true;
            _queuedCoroutines.Enqueue(PlayMusicFade(_audioSource, _lastClip, loop, fadeTime, _lastTime));
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
        source.volume = 0.2f;
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
    }

    private IEnumerator FadeIn(AudioSource audioSource, float fadeTime)
    {
        float startVolume = Math.Min(0.2f, SettingsManager.Instance.MusicVolume);

        audioSource.volume = 0;
        audioSource.Play();

        while (audioSource.volume < SettingsManager.Instance.MusicVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        audioSource.volume = SettingsManager.Instance.MusicVolume;
    }
}
