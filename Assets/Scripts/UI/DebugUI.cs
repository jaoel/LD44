using UnityEngine;
using System.Collections;

public class DebugUI : MonoBehaviour
{
    private bool _show = false;
    private int _lastLine = 50;
    private int _line = 50;
    private int _controlWidth = 150;

    private void DrawGUI()
    {
        GUI.Box(new Rect(10, 30, _controlWidth + 20, _lastLine), "");

        DrawTeleportToStairsButton();
    }

    private void DrawTeleportToStairsButton()
    {
        if (GUI.Button(NextControlRect(), "Teleport to stairs"))
        {
            Stairs stairs = FindObjectOfType<Stairs>();
            Player player = Main.Instance.player;
            if (stairs != null && player != null)
            {
                Vector3 position = player.transform.position;
                position.x = stairs.transform.position.x;
                position.y = stairs.transform.position.y;
                player.transform.position = position;
                CameraManager.Instance.SetCameraPosition(player.transform.position);
            }
            else
            {
                Debug.LogWarning("Unable to find stairs or player.");
            }
        }
    }

    private Rect NextControlRect(int height = 20)
    {
        Rect rect = new Rect(20, _line, _controlWidth, height);
        _line += height;
        return rect;
    }

    private void DrawToggleDebugMenuButton()
    {
        if (GUI.Button(new Rect(20, 10, _controlWidth, 20), _show ? "Hide Debug Menu" : "Show Debug Menu"))
        {
            _show = !_show;
            PlayerPrefs.SetInt(PlayerPrefsStrings.ShowDebugMenu, _show ? 1 : 0);
        }
    }

    private void Awake()
    {
        _show = PlayerPrefs.GetInt(PlayerPrefsStrings.ShowDebugMenu, 0) != 0 ? true : false;
    }

    private void OnGUI()
    {
        Main main = Main.Instance;
        if (main.Paused)
        {
            DrawToggleDebugMenuButton();
            _line = 50;
            if (_show)
            {
                DrawGUI();
            }
            _lastLine = _line;
        }
    }
}
