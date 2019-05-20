using UnityEngine;

public class DoorAnimationController : MonoBehaviour
{
    public Door door;

    public void OnDoorClosed()
    {
        door.Close(false);    
    }

    public void OnDoorOpened()
    {
        door.Open(true);
    }
}
