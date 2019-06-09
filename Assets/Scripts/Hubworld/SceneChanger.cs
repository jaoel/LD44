using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SceneChanger : MonoBehaviour
{
    [SerializeField]
    private string _scene;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SceneManager.LoadScene(_scene, LoadSceneMode.Single);
    }
}
