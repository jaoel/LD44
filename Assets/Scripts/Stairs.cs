using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Stairs : MonoBehaviour
    {
        public bool isShop = true;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerContainer.Instance.Layers["Player"])
            {
                if (isShop) {
                    Main.Instance.GenerateShop();
                } else {
                    Main.Instance.GenerateMap();
                }
                Destroy(gameObject);
            }
        }
    }
}
