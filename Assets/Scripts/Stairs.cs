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
        int _playerLayerMask = LayerMask.NameToLayer("Player");
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == _playerLayerMask)
            {
                Debug.Log("ayy lmao");
            }
        }
    }
}
