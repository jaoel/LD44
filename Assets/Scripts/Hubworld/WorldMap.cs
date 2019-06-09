using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldMap : MonoBehaviour
{
    [SerializeField]
    private GameObject _dungeonMap;

    [SerializeField]
    private GameObject _hubContainer;

    [SerializeField]
    private GameObject _playerContainer;

    [SerializeField]
    private List<OverworldDungeon> _dungeons;

    [SerializeField]
    private TMPro.TextMeshProUGUI _levelText;

    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        OverworldDungeon highlighted = null;

        _dungeons.ForEach(x =>
        {
            if (x.collider.bounds.Contains(mousePos.ToVector2()))
            {
                highlighted = x;
            }
        });

        if (highlighted == null)
        {
            _levelText.text = "";
        }
        else if (!_levelText.text.Equals(highlighted.name))
        {
            _levelText.text = highlighted.name;
        }

        if (highlighted != null)
        {
            _levelText.transform.parent.position = new Vector3(Camera.main.WorldToScreenPoint(highlighted.collider.bounds.center).x,
              Camera.main.WorldToScreenPoint(highlighted.collider.bounds.min).y, 0.0f);

            if (Input.GetMouseButton(0))
            {
                SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            }
        }

        if (Input.GetKey(KeyCode.P))
        {
            Return();
        }
    }

    public void Return()
    {
        _hubContainer.SetActive(true);
        _playerContainer.SetActive(true);
        _dungeonMap.SetActive(false);

        Cursor.visible = false;
    }
}
