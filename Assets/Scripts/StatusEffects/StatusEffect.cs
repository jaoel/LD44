using UnityEngine;

public class StatusEffect : MonoBehaviour
{
    public bool OverrideNavigation => _overrideNavigation;

    [SerializeField]
    protected float _lifetime;

    [SerializeField]
    protected bool _overrideNavigation;

    protected IBuffable _owner;
    protected GameObject _ownerGameObject;
    protected SpriteRenderer _ownerRenderer;
    protected Navigation _navigation;

    protected float _currentLifetime;

    public virtual void Awake()
    {
        _currentLifetime = _lifetime;
    }

    public virtual void OnApply(IBuffable owner, GameObject ownerGameObject, SpriteRenderer ownerRenderer, 
        Navigation navigation = null)
    {
        _owner = owner;
        _ownerGameObject = ownerGameObject;
        _ownerRenderer = ownerRenderer;
        _navigation = navigation;
    }

    public virtual void Navigation()
    {

    }

    protected virtual void FixedUpdate()
    {
        _currentLifetime -= Time.deltaTime;

        if (_currentLifetime <= 0.0f)
        {
            BeforeDestroyed();
        }
    }

    protected virtual void BeforeDestroyed()
    {
        Destroy(this);
    }
}
