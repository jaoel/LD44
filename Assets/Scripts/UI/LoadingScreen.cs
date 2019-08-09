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

    [SerializeField]
    private RectTransform _playerImageTransform;

    [SerializeField]
    private TMPro.TextMeshProUGUI _loadingText;

    [SerializeField]
    private CanvasGroup _loadingScreenContainer;

    private AsyncOperation _asyncOp;

    private float _minLoadTimeSec;
    private float _startTime;

    private LoadingState _loadingState;
    private Tween _fadeTween;
    private float _fadeTime;
    private bool _loadingDone;

    private int _loadingTextFrame;

    private void Start()
    {
        _loadingTextFrame = 0;
        _loadingState = LoadingState.NOT_LOADING;
        _loadingDone = true;
        _fadeTween = null;
        _asyncOp = null;
        _minLoadTimeSec = 5.0f;
        _startTime = 0.0f;
    }

    private void Update()
    {
        if (_loadingState == LoadingState.NOT_LOADING)
        {
            return;
        }

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

    public void StartLoad(Func<AsyncOperation> onLoad, float fadeTime = 0.5f, float minLoadTime = 2.0f)
    {
        _playerImageTransform.localPosition = new Vector3(-25.0f, 0.0f, 0.0f);
        _startTime = Time.realtimeSinceStartup;
        _loadingTextFrame = 0;
        _minLoadTimeSec = minLoadTime;
        StartCoroutine(Load(onLoad, fadeTime));
    }

    private IEnumerator Load(Func<AsyncOperation> onLoad, float fadeTime)
    {
        while (!_loadingDone)
        {
            yield return new WaitForSeconds(0.1f);
        }

        _loadingState = LoadingState.LOADING;
        StartCoroutine(AnimateLoadingText());

        Time.timeScale = 0.0f;
        _fadeTime = fadeTime;

        _fadeTween = _loadingScreenContainer.DOFade(1.0f, fadeTime).SetUpdate(true);

        _fadeTween.onComplete = () =>
        {
            if (onLoad != null)
            {
                _asyncOp = onLoad();
            }

            if (_asyncOp == null)
            {
                _loadingState = LoadingState.DONE;
                _startTime = Time.realtimeSinceStartup;
            }
        };

        yield break;
    }

    private IEnumerator AnimateLoadingText()
    {
        while (_loadingState != LoadingState.NOT_LOADING)
        {
            _loadingTextFrame++;
            _loadingTextFrame = _loadingTextFrame % 4;

            _loadingText.text = "Loading";

            for (int i = 0; i < _loadingTextFrame; i++)
            {
                _loadingText.text += ".";
            }

            yield return new WaitForSecondsRealtime(0.5f);
        }

        yield break;
    }

    private void Loading()
    {
        if (_asyncOp != null)
        {
            _playerImageTransform.DOLocalMoveX(Utility.ConvertRange(0.0f, 1.0f, -25.0f, 75.0f, _asyncOp.progress), 0.5f).SetUpdate(true);

            if (_asyncOp.isDone)
            {
                _loadingState = LoadingState.DONE;
                _asyncOp = null;
            }
        }
    }

    private void LoadDone()
    {
        float elapsedTime = Time.realtimeSinceStartup - _startTime;
        if (elapsedTime >= _minLoadTimeSec && elapsedTime > _fadeTime)
        {
            _fadeTween = _loadingScreenContainer.DOFade(0.0f, _fadeTime).SetUpdate(true);

            _loadingScreenContainer.DOFade(0.0f, _fadeTime).SetUpdate(true);

            _startTime = 0.0f;

            _fadeTween.onComplete = () =>
            {
                Time.timeScale = 1.0f;
                _loadingDone = true;
                _loadingState = LoadingState.NOT_LOADING;
            };
        }
    }
}
