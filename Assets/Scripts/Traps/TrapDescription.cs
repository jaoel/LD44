using UnityEngine;

[CreateAssetMenu]
public class TrapDescription : ScriptableObject
{
    public Sprite nonTriggeredSprite;
    public Sprite triggeredSprite;
    public AudioClip triggerSound;
    public AudioClip resetSound;
    public bool resetOnExit;
    public bool triggerOnInvulnerable;
    public float resetTime;

    public int damage;
    public float slowFactor;
    public float slowDuration;
}
