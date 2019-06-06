using UnityEngine;

public class BurningDebuff : StatusEffect
{
    [SerializeField]
    protected int _damagePerTick;

    [SerializeField]
    protected float _tickIntervals;

    [SerializeField]
    private GameObject _fireParticleSystemPrefab;

    private ParticleSystem _fireParticleInstance;

    private float _timePassed;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnApply(IBuffable owner, GameObject ownerGameObject, SpriteRenderer ownerRenderer)
    {
        base.OnApply(owner, ownerGameObject, ownerRenderer);

        _fireParticleInstance = Instantiate(_fireParticleSystemPrefab, ownerGameObject.transform).GetComponent<ParticleSystem>();

        ParticleSystem.ShapeModule shape = _fireParticleInstance.shape;
        shape.spriteRenderer = ownerRenderer;

        _timePassed = _tickIntervals;
    }

    protected override void FixedUpdate()
    {
        _timePassed += Time.deltaTime;
        if (_timePassed >= _tickIntervals)
        {
            _timePassed = 0.0f;
            if (_owner.ReceiveDamage(_damagePerTick, Vector2.zero, false, false))
            {
                BeforeDestroyed();
            }
        }

        base.FixedUpdate();
    }

    protected override void BeforeDestroyed()
    {
        var main = _fireParticleInstance.main;
        main.loop = false;
        Destroy(_fireParticleInstance, _fireParticleInstance.main.duration);
        base.BeforeDestroyed();
    }
}
