using System;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    public bool isShop = true;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == Layers.Player)
        {
            Func<AsyncOperation> onLoad = () =>
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

                return null;
            };

            GameSceneManager.Instance.FadeScreen(onLoad);
        }
    }
}
