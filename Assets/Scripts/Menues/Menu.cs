using UnityEngine;

public abstract class Menu : MonoBehaviour
{
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public abstract void OnPressedEscape();
}
