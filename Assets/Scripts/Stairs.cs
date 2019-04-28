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
        public Main _main;
        public bool isShop = true;

        public void Awake()
        {
            _main = GameObject.Find("Main Camera").GetComponent<Main>();
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerContainer.Instance.Layers["Player"])
            {
                if (isShop) {
                    _main.GenerateShop();
                } else {
                    _main.GenerateMap();
                }
                Destroy(gameObject);
            }
        }
    }
}
