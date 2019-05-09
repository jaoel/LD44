using UnityEngine;
using System.Collections;

public class ParticleSelfDestruct : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, GetComponent<ParticleSystem>().main.startLifetime.constantMax);
    }
}