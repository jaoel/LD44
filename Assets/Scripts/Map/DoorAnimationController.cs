using UnityEngine;

public class DoorAnimationController : MonoBehaviour
{
    public Door door;

    public void OnDoorClosed()
    {

    }

    public void OnDoorOpened()
    {
        door.Unlock();
    }
}
