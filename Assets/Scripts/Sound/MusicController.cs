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

    private bool _fadingMusic = false;

    private AudioClip _lastClip;
    private float _lastTime;

    private Queue<IEnumerator> _queuedCoroutines;
    private bool _playingGameplayMusic;

    private static MusicController _instance = null;
    public static MusicController Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<MusicController>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a MusicController");
                return null;
            }

            _instance._queuedCoroutines = new Queue<IEnumerator>();
            _instance.StartCoroutine(_instance.ProcessCoroutines());

            DontDestroyOnLoad(_instance.gameObject);
            return _instance;
        }
    }

    public void SetVolume()
    {
        _audioSource.volume = SettingsManager.Instance.MusicVolume;
    }

    private void Awake()
    {
    }

    private void Start()
    {
        SetVolume();
    }

    private IEnumerator ProcessCoroutines()
    {
        while (true)
        {
            if (_queuedCoroutines != null)
            {
                while (_queuedCoroutines.Count > 0)
                {
                    while (_fadingMusic)
                    {
                        yield return new WaitForSeconds(0.01f);
                    }

                    yield return StartCoroutine(_queuedCoroutines.Dequeue());
                }

            }
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        if(_playingGameplayMusic && !_fadingMusic && !_audioSource.isPlaying && _queuedCoroutines.Count == 0)
        {
            PlayMusic("RandomGameplay", false, 1.0f);
        }
    }

    public void PlayMusic(string key, bool loop = true, float fadeTime = 0.2f)
    {
        if (key == "MainMenu")
        {
            _queuedCoroutines.Enqueue(PlayMusicFade(_audioSource, menuMusic, loop, fadeTime));
        }
        else if (key == "RandomGameplay")
        {
            _queuedCoroutines.Enqueue(PlayMusicFade(_audioSource, gameMusic[UnityEngine.Random.Range(0, gameMusic.Count)],
                loop, fadeTime, 0.0f, true));
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
            _queuedCoroutines.Enqueue(PlayMusicFade(_audioSource, _lastClip, loop, fadeTime, _lastTime, true));
        }
    }

    private IEnumerator PlayMusicFade(AudioSource source, AudioClip clip, bool loop, float fadeTime, float time = 0.0f, bool gameplayMusic = false)
    {
        _playingGameplayMusic = gameplayMusic;
        _fadingMusic = true;
        float fadeHalfTime = fadeTime / 2.0f;

        IEnumerator fadeOut = null;
        if (source.isPlaying)
        {
            fadeOut = FadeOut(source, fadeHalfTime);
            StartCoroutine(fadeOut);
            yield return new WaitForSeconds(fadeHalfTime);
        }

        if (fadeOut != null)
        {
            while (fadeOut.MoveNext())
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        source.clip = clip;
        source.loop = loop;
        source.volume = 0.2f;
        source.time = time;
        source.Play();

        StartCoroutine(FadeIn(source, fadeHalfTime));
        yield break;
    }

    private IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;
        float timePassed = 0.0f;
        while (audioSource.volume > 0 || timePassed <= fadeTime)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            timePassed += Time.deltaTime;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    private IEnumerator FadeIn(AudioSource audioSource, float fadeTime)
    {
        float startVolume = Math.Min(0.2f, SettingsManager.Instance.MusicVolume);

        audioSource.volume = 0;
        audioSource.Play();

        float timePassed = 0.0f;
        while (audioSource.volume < SettingsManager.Instance.MusicVolume || timePassed <= fadeTime)
        {
            audioSource.volume += startVolume * Time.deltaTime / fadeTime;
            timePassed += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = SettingsManager.Instance.MusicVolume;
        _fadingMusic = false;
    }
}
