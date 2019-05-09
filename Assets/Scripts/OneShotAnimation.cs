using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class OneShotAnimation : MonoBehaviour
{
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void AnimationEnded()
    {
        _animator.StopPlayback();
        Destroy(gameObject);
    }
}
