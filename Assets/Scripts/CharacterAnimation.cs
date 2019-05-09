using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class CharacterAnimation : MonoBehaviour
{
    public enum AnimationType
    {
        Idle,
        Run,
        Attack,
        Die
    }

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateAnimation(AnimationType type, Vector2 direction)
    {
        int directionIndex = Mathf.RoundToInt((135f + Vector2.SignedAngle(new Vector2(1f, 1f).normalized, direction)) / 90f);
        int indexOffset = 0;
        switch (type)
        {
            case AnimationType.Run:
                indexOffset = 1;
                break;
            case AnimationType.Attack:
                indexOffset = 5;
                break;
            case AnimationType.Die:
                indexOffset = 9;
                break;
        }

        _animator.SetInteger("direction", directionIndex + indexOffset);
    }

}
