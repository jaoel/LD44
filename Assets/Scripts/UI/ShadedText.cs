using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class ShadedText : MonoBehaviour {

    public float offset = 1;
    public Color colorLeft = Color.black;
    public Color colorRight = Color.black;
    public Color colorUp = Color.black;
    public Color colorDown = Color.black;

    private TMPro.TextMeshProUGUI textMesh;
    private TMPro.TextMeshProUGUI shadowLeft;
    private TMPro.TextMeshProUGUI shadowRight;
    private TMPro.TextMeshProUGUI shadowUp;
    private TMPro.TextMeshProUGUI shadowDown;

    private string oldText = "";
    private bool initialized = false;

    void Awake() {
        textMesh = GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void Start() {

        shadowLeft = Instantiate(textMesh, transform.parent);
        Destroy(shadowLeft.GetComponent<ShadedText>());
        shadowLeft.color = colorLeft;
        shadowLeft.rectTransform.anchoredPosition = shadowLeft.rectTransform.anchoredPosition + Vector2.left * offset;

        shadowRight = Instantiate(textMesh, transform.parent);
        Destroy(shadowRight.GetComponent<ShadedText>());
        shadowRight.color = colorRight;
        shadowRight.rectTransform.anchoredPosition = shadowRight.rectTransform.anchoredPosition + Vector2.right * offset;

        shadowUp = Instantiate(textMesh, transform.parent);
        Destroy(shadowUp.GetComponent<ShadedText>());
        shadowUp.color = colorUp;
        shadowUp.rectTransform.anchoredPosition = shadowUp.rectTransform.anchoredPosition + Vector2.up * offset;

        shadowDown = Instantiate(textMesh, transform.parent);
        Destroy(shadowDown.GetComponent<ShadedText>());
        shadowDown.color = colorDown;
        shadowDown.rectTransform.anchoredPosition = shadowDown.rectTransform.anchoredPosition + Vector2.down * offset;

        transform.SetAsLastSibling();

        initialized = true;
    }

    private void SetText(string text) {
        shadowLeft.text = text;
        shadowRight.text = text;
        shadowUp.text = text;
        shadowDown.text = text;
    }

    private void Update() {
        if(oldText != textMesh.text) {
            oldText = textMesh.text;
            SetText(textMesh.text);
        }
    }

    private void SetActiveAll(bool active) {
        if (!initialized) return;

        shadowLeft.gameObject.SetActive(active);
        shadowRight.gameObject.SetActive(active);
        shadowUp.gameObject.SetActive(active);
        shadowDown.gameObject.SetActive(active);
    }

    private void OnDisable() {
        SetActiveAll(false);
    }

    private void OnEnable() {
        SetActiveAll(true);
    }
}
