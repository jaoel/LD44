using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ShopTrigger : InteractiveObject
{
    public Item item;
    [SerializeField]
    private ShopKeeper shopKeeper;
    private GameObject _speechBubble;
    private bool _activated;

    public override void OnActivate()
    {
        if (_activated)
        {
            return;
        }

        _activated = true;
        item.transform.position = Main.Instance.player.transform.position;
        

        base.OnActivate();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (item == null)
        {
            return;
        }

        _speechBubble = shopKeeper.SpawnSpeechBubble(item.GetShopkeeperDescription());
        base.OnTriggerEnter2D(collision);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        Destroy(_speechBubble);
        base.OnTriggerExit2D(collision);
    }
}
