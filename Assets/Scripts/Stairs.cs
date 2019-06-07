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
                Main.Instance.GenerateShop();
            }
            else
            {
                Main.Instance.GenerateMap();
            }
        }
    }
}
