using UnityEngine;

public class Stairs : MonoBehaviour
{
    public bool isShop = true;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == Layers.Player)
        {
            if (isShop)
            {
                MapManager.Instance.GenerateShop();
            }
            else
            {
                MapManager.Instance.GenerateMap();
            }
        }
    }
}
