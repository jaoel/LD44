using UnityEngine;

class Keybindings {
    public static float MoveLeft => (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ? 1f : 0f;
    public static float MoveRight => (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ? 1f : 0f;
    public static float MoveUp => (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? 1f : 0f;
    public static float MoveDown => (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ? 1f : 0f;
    public static bool Attack => Input.GetMouseButton(0);
    public static Vector3 MousePosition => Input.mousePosition;
}
