using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class ShadedText : MonoBehaviour {

    public Color colorLeft = Color.black;
    public Color colorRight = Color.black;
    public Color colorUp = Color.black;
    public Color colorDown = Color.black;

    private TMPro.TextMeshProUGUI textMesh;
    private TMPro.TextMeshProUGUI shadowLeft;
    private TMPro.TextMeshProUGUI shadowRight;
    private TMPro.TextMeshProUGUI shadowUp;
    private TMPro.TextMeshProUGUI shadowDown;

    // Use this for initialization
    void Awake() {
        textMesh = GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void Start() {

        shadowLeft = Instantiate(textMesh, transform.parent);
        Destroy(shadowLeft.GetComponent<ShadedText>());
        shadowLeft.color = colorLeft;
        shadowLeft.rectTransform.position = shadowLeft.rectTransform.position + Vector3.left;

        shadowRight = Instantiate(textMesh, transform.parent);
        Destroy(shadowRight.GetComponent<ShadedText>());
        shadowRight.color = colorRight;
        shadowRight.rectTransform.position = shadowRight.rectTransform.position + Vector3.right;

        shadowUp = Instantiate(textMesh, transform.parent);
        Destroy(shadowUp.GetComponent<ShadedText>());
        shadowUp.color = colorUp;
        shadowUp.rectTransform.position = shadowUp.rectTransform.position + Vector3.up;

        shadowDown = Instantiate(textMesh, transform.parent);
        Destroy(shadowDown.GetComponent<ShadedText>());
        shadowDown.color = colorDown;
        shadowDown.rectTransform.position = shadowDown.rectTransform.position + Vector3.down;

        transform.SetAsLastSibling();
    }

    private string oldText = "";

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
}
