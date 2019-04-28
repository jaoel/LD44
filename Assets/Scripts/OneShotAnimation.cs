using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class OneShotAnimation : MonoBehaviour {
    Animator animator;

    void Start() {
        animator = GetComponent<Animator>();
    }

    public void AnimationEnded() {
        animator.StopPlayback();
        Destroy(gameObject);
    }
}
