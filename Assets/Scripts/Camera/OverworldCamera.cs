using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OverworldCamera : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    private void Update()
    {
        Vector3 direction = Vector3.zero;

        if (Keybindings.MoveLeft > 0.0f)
        {
            direction += Vector3.left;
        }
        else if (Keybindings.MoveRight > 0.0f)
        {
            direction += Vector3.right;
        }

        if (Keybindings.MoveUp > 0.0f)
        {
            direction += Vector3.up;
        }
        else if (Keybindings.MoveDown > 0.0f)
        {
            direction += Vector3.down;
        }

        transform.position += direction.normalized * _speed * Time.deltaTime;
    }
}
