using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    private bool _wasPaused = false;
    private bool _show = false;
    private int _lastLine = 50;
    private int _line = 50;
    private int _controlWidth = 150;
    private bool _giveItemDropdownExpanded = false;
    private string _currentSeed = "";
    private string _currentLevel = "0";

    private void DrawGUI()
    {
        GUI.Box(new Rect(10, 30, _controlWidth + 20, _lastLine), "");

        DrawTeleportToStairsButton();
        DrawGiveItemDropdown();
        DrawGodModeCheckbox();
        DrawHealButton();

        GUICustom.Separator(NextControlRect(10));

        DrawLevelGeneratorSetSeed();

        GUICustom.Separator(NextControlRect(10));

        DrawToggleDoors();
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

    private void DrawGiveItemDropdown()
    {
        List<Item> itemPrefabs = ItemManager.Instance.ToList();
        List<string> itemNames = itemPrefabs.Select(item => item.name).ToList();
        int selectedItem = -1;
        _giveItemDropdownExpanded = GUICustom.Dropdown(NextControlRect(), "Give Item", _giveItemDropdownExpanded, itemNames, ref selectedItem);
        if(selectedItem != -1)
        {
            Item item = Instantiate(itemPrefabs[selectedItem]);
            Player player = Main.Instance.player;
            if(item && player)
            {
                item.Apply(player.gameObject);
            }
            _giveItemDropdownExpanded = true;
        }
    }

    private void DrawGodModeCheckbox()
    {
        Player player = Main.Instance.player;
        if (player) {
            if(GUI.Toggle(NextControlRect(), player.GodMode, "God Mode"))
            {
                player.GodMode = true;
            }
            else
            {
                player.GodMode = false;
            }
        }
    }

    private void DrawHealButton()
    {
        Player player = Main.Instance.player;
        if (player)
        {
            if (GUI.Button(NextControlRect(), "Heal Player"))
            {
                player.Health = player.MaxHealth;
            }
        }
    }

    private void DrawLevelGeneratorSetSeed()
    {
        GUI.Label(NextControlRect(), "Seed");
        _currentSeed = GUI.TextField(NextControlRect(), _currentSeed);
        GUI.Label(NextControlRect(), "Level");
        _currentLevel = GUI.TextField(NextControlRect(), _currentLevel);
        if (GUI.Button(NextControlRect(), "Restart with Seed"))
        {
            if(long.TryParse(_currentSeed, out long newSeed))
            {
                if(int.TryParse(_currentLevel, out int currentLevel))
                {
                    Main.Instance.CurrentLevel = Mathf.Max(0, currentLevel);
                }
                else
                {
                    _currentLevel = (Main.Instance.CurrentLevel - 1).ToString();
                }
                Main.Instance.GenerateMapWithSeed(newSeed);
            }
            else
            {
                Debug.LogWarning(_currentSeed + " is not a valid seed.");
                _currentSeed = MapGenerator.Instance.GetCurrentSeed().ToString();
            }
        }
    }

    private void DrawToggleDoors()
    {
        if (GUI.Button(NextControlRect(), "Open All Doors"))
        {
            Door[] doors = FindObjectsOfType<Door>();
            foreach(Door door in doors)
            {
                door.ToggleClosed(false);
            }
        }

        if (GUI.Button(NextControlRect(), "Close All Doors"))
        {
            Door[] doors = FindObjectsOfType<Door>();
            foreach (Door door in doors)
            {
                door.ToggleClosed(true);
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

    private void OnShow()
    {
        _currentSeed = MapGenerator.Instance.GetCurrentSeed().ToString();
        _currentLevel = (Main.Instance.CurrentLevel - 1).ToString();
    }

    private void OnGUI()
    {
        Main main = Main.Instance;
        if (main.Paused)
        {
            if (!_wasPaused)
            {
                OnShow();
            }

            DrawToggleDebugMenuButton();
            _line = 50;
            if (_show)
            {
                DrawGUI();
            }
            _lastLine = _line;
        }
        _wasPaused = main.Paused;
    }
}
