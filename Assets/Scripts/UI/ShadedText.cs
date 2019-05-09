using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class ShadedText : MonoBehaviour
{

    public float offset = 1;
    public Color colorLeft = Color.black;
    public Color colorRight = Color.black;
    public Color colorUp = Color.black;
    public Color colorDown = Color.black;

    private TMPro.TextMeshProUGUI _textMesh;
    private TMPro.TextMeshProUGUI _shadowLeft;
    private TMPro.TextMeshProUGUI _shadowRight;
    private TMPro.TextMeshProUGUI _shadowUp;
    private TMPro.TextMeshProUGUI _shadowDown;

    private string _oldText = "";
    private bool _initialized = false;

    void Awake()
    {
        _textMesh = GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void Start()
    {

        _shadowLeft = Instantiate(_textMesh, transform.parent);
        Destroy(_shadowLeft.GetComponent<ShadedText>());
        _shadowLeft.color = colorLeft;
        _shadowLeft.rectTransform.anchoredPosition = _shadowLeft.rectTransform.anchoredPosition + Vector2.left * offset;

        _shadowRight = Instantiate(_textMesh, transform.parent);
        Destroy(_shadowRight.GetComponent<ShadedText>());
        _shadowRight.color = colorRight;
        _shadowRight.rectTransform.anchoredPosition = _shadowRight.rectTransform.anchoredPosition + Vector2.right * offset;

        _shadowUp = Instantiate(_textMesh, transform.parent);
        Destroy(_shadowUp.GetComponent<ShadedText>());
        _shadowUp.color = colorUp;
        _shadowUp.rectTransform.anchoredPosition = _shadowUp.rectTransform.anchoredPosition + Vector2.up * offset;

        _shadowDown = Instantiate(_textMesh, transform.parent);
        Destroy(_shadowDown.GetComponent<ShadedText>());
        _shadowDown.color = colorDown;
        _shadowDown.rectTransform.anchoredPosition = _shadowDown.rectTransform.anchoredPosition + Vector2.down * offset;

        transform.SetAsLastSibling();

        _initialized = true;
    }

    private void SetText(string text)
    {
        _shadowLeft.text = text;
        _shadowRight.text = text;
        _shadowUp.text = text;
        _shadowDown.text = text;
    }

    private void Update()
    {
        if (_oldText != _textMesh.text)
        {
            _oldText = _textMesh.text;
            SetText(_textMesh.text);
        }
    }

    private void SetActiveAll(bool active)
    {
        if (!_initialized)
        {
            return;
        }

        _shadowLeft.gameObject.SetActive(active);
        _shadowRight.gameObject.SetActive(active);
        _shadowUp.gameObject.SetActive(active);
        _shadowDown.gameObject.SetActive(active);
    }

    private void OnDisable()
    {
        SetActiveAll(false);
    }

    private void OnEnable()
    {
        SetActiveAll(true);
    }
}
