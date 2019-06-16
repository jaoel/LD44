using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    [SerializeField]
    private GameObject speechBubblePrefab;

    [SerializeField]
    private GameObject shopUI;

    private List<GameObject> _speechBubbles = new List<GameObject>();

    public GameObject SpawnSpeechBubble(string text)
    {
        GameObject go = GameObject.Instantiate(speechBubblePrefab, Camera.main.WorldToScreenPoint(transform.position),
            Quaternion.identity, shopUI.transform);

        go.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = text;

        _speechBubbles.Add(go);
        return go;
    }

    private void LateUpdate()
    {
        for (int i = _speechBubbles.Count - 1; i >= 0; i--)
        {
            if (_speechBubbles[i] == null)
            {
                _speechBubbles.RemoveAt(i);
                continue;
            }

            _speechBubbles[i].transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0.0f, 2.0f, 0.0f));
        }
    }
}
