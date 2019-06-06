using UnityEngine;

public class StatusEffect : MonoBehaviour
{
    [SerializeField]
    protected float _lifetime;

    [SerializeField]
    protected bool _overrideNavigation;

    protected IBuffable _owner;
    protected GameObject _ownerGameObject;
    protected SpriteRenderer _ownerRenderer;

    protected float _currentLifetime;

    public virtual void Awake()
    {
        _currentLifetime = _lifetime;
    }

    public virtual void OnApply(IBuffable owner, GameObject ownerGameObject, SpriteRenderer ownerRenderer)
    {
        _owner = owner;
        _ownerGameObject = ownerGameObject;
        _ownerRenderer = ownerRenderer;
    }

    protected virtual void FixedUpdate()
    {
        _currentLifetime -= Time.deltaTime;

        if (_currentLifetime <= 0.0f)
        {
            BeforeDestroyed();
            Destroy(this);
        }
    }

    protected virtual void BeforeDestroyed()
    {

    }
}
