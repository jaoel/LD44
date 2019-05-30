using UnityEngine;

public abstract class Trap : MonoBehaviour
{
    public TrapDescription description;
    public AudioSource audioSource;
    public SpriteRenderer spriteRenderer;
    public GameObject minimapIcon;

    protected bool _triggered;
    protected float _triggerTimer;

    public virtual void ApplyEffect(Player player, GameObject playerGO)
    {
        audioSource.volume = SettingsManager.Instance.SFXVolume;
        audioSource.PlayOneShot(description.triggerSound);
        spriteRenderer.sprite = description.triggeredSprite;
        _triggered = true;
    }
    public virtual void Apply(GameObject playerGO)
    {
        if (_triggered)
        {
            return;
        }

        Player player = playerGO.GetComponent<Player>();
        if (player != null && (!player.IsInvulnerable || description.triggerOnInvulnerable))
        {
            ApplyEffect(player, playerGO);
            minimapIcon.SetActive(false);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (_triggered && description.resetOnExit && _triggerTimer > 0.0f)
        {
            _triggerTimer -= Time.deltaTime;

            if (_triggerTimer <= 0.0f)
            {
                ResetTrap();
                _triggered = false;
                _triggerTimer = 0.0f;
            }
        }
    }

    protected virtual void ResetTrap()
    {
        audioSource.PlayOneShot(description.resetSound);
        spriteRenderer.sprite = description.nonTriggeredSprite;
        minimapIcon.SetActive(true);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            Apply(collision.gameObject);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject && description.resetOnExit && _triggered)
        {
            _triggerTimer = description.resetTime;
        }
    }
}