using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public enum LoadingState
{
    LOADING,
    DONE,
    NOT_LOADING
}

public class LoadingScreen : MonoBehaviour
{
    public LoadingState LoadingState => _loadingState;

    [SerializeField]
    private Image _fadeImage;

    private AsyncOperation _asyncOp;

    private float _minLoadTimeSec;
    private float _timeSpentLoadingSec;

    private LoadingState _loadingState;
    private Tween _fadeTween;
    private float _fadeTime;
    private bool _loadingDone;

    private void Start()
    {
        _loadingState = LoadingState.NOT_LOADING;
        _loadingDone = true;
        _fadeTween = null;
        _asyncOp = null;
        _minLoadTimeSec = 1.0f;
        _timeSpentLoadingSec = 0.0f;
    }

    private void Update()
    {
        _timeSpentLoadingSec += Time.deltaTime;

        switch (_loadingState)
        {
            case LoadingState.LOADING:
                Loading();
                break;
            case LoadingState.DONE:
                LoadDone();
                break;
            default:
                break;
        }
    }

    public void StartLoad(Func<AsyncOperation> onLoad, float fadeTime = 0.5f, float minLoadTime = 1.0f)
    {
        _minLoadTimeSec = minLoadTime;
        StartCoroutine(Load(onLoad, fadeTime));
    }

    private IEnumerator Load(Func<AsyncOperation> onLoad, float fadeTime)
    {
        while(!_loadingDone)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Time.timeScale = 0.0f;
        _fadeTime = fadeTime;

        _fadeTween = DOTween.To(() => _fadeImage.color, x => _fadeImage.color = x, new Color(0.0f, 0.0f, 0.0f, 1.0f), fadeTime).SetUpdate(true);

        _fadeTween.onComplete = () =>
        {
            if (onLoad != null)
            {
                _asyncOp = onLoad();
            }

            if (_asyncOp == null)
            {
                _loadingState = LoadingState.DONE;
                _timeSpentLoadingSec = float.MaxValue;
            }
        };

        _loadingState = LoadingState.LOADING;

        yield break; ;
    }

    private void Loading()
    {
        if (_asyncOp != null)
        {
            if (_asyncOp.isDone)
            {
                _loadingState = LoadingState.DONE;
                _asyncOp = null;
            }
        }
    }

    private void LoadDone()
    {
        if (_timeSpentLoadingSec >= _minLoadTimeSec && _timeSpentLoadingSec > _fadeTime)
        {
            _fadeTween = DOTween.To(() => _fadeImage.color, x => _fadeImage.color = x, new Color(0.0f, 0.0f, 0.0f, 0.0f), _fadeTime).SetUpdate(true);
            _timeSpentLoadingSec = 0.0f;

            _fadeTween.onComplete = () =>
            {
                Time.timeScale = 1.0f;
                _loadingDone = true;
                _loadingState = LoadingState.NOT_LOADING;
            };
        }
    }
}
