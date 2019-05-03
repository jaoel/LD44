using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DustParticleSpawner : MonoBehaviour
{
    public ParticleSystem dustTrail;
    public Rigidbody2D rigidBody;

    private void FixedUpdate()
    {
        if (rigidBody.velocity.magnitude <= 0.0f)
        {
            dustTrail.Stop();
        }
        else
        {
            if (!dustTrail.isPlaying)
            {
                dustTrail.Play(true);
            }

            if (rigidBody.velocity.x > 0.0f)
                dustTrail.transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
            else if (rigidBody.velocity.x < 0.0f)
                dustTrail.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);

            if (rigidBody.velocity.y < 0.0f)
                dustTrail.GetComponent<Renderer>().sortingLayerName = "Character";
            else
                dustTrail.GetComponent<Renderer>().sortingLayerName = "Particles";
        }
    }
}
