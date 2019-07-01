using UnityEngine;

class Keybindings {
    public static float MoveLeft => InputModes.HasFlag(InputMode.Player) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ? 1f : 0f;
    public static float MoveRight => InputModes.HasFlag(InputMode.Player) && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ? 1f : 0f;
    public static float MoveUp => InputModes.HasFlag(InputMode.Player) && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? 1f : 0f;
    public static float MoveDown => InputModes.HasFlag(InputMode.Player) && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ? 1f : 0f;
    public static bool Attack => InputModes.HasFlag(InputMode.Player) && Input.GetMouseButton(0);
    public static bool Reload => InputModes.HasFlag(InputMode.Player) && Input.GetKey(KeyCode.R);
    public static bool Use => InputModes.HasFlag(InputMode.Player) && Input.GetKey(KeyCode.E);
    public static bool WeaponSlot1 => InputModes.HasFlag(InputMode.Player) && Input.GetKey(KeyCode.Alpha1);
    public static bool WeaponSlot2 => InputModes.HasFlag(InputMode.Player) && Input.GetKey(KeyCode.Alpha2);

    public static Vector3 MousePosition => Input.mousePosition;
    public static bool UIExpandMinimap => (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.M));

    public static InputMode InputModes { get; private set; } = InputMode.Player;

    public static void ActivateInputMode(InputMode inputMode)
    {
        InputModes |= inputMode;
        Debug.Log(System.Convert.ToString((int)InputModes, 2));
    }

    public static void DeactivateInputMode(InputMode inputMode)
    {
        InputModes &= ~inputMode;
        Debug.Log(System.Convert.ToString((int)InputModes, 2));
    }

    [System.Flags]
    public enum InputMode
    {
        None = 0,
        Player = 1,
        Minimap = 2,
    }
}
