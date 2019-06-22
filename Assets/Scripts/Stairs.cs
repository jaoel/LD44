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
                Main.Instance.sessionData.UpdatePlayerData(collision.gameObject.GetComponent<Player>());
                MapManager.Instance.GenerateShop();
            }
            else
            {
                MapManager.Instance.GenerateMap();
            }
        }
    }
}
