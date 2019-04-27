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
        int _playerLayerMask;

        public void Awake()
        {
            _playerLayerMask = LayerMask.NameToLayer("Player");
            _main = GameObject.Find("Main Camera").GetComponent<Main>();
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == _playerLayerMask)
            {
                _main.GenerateMap();
                Destroy(gameObject);
            }
        }
    }
}
