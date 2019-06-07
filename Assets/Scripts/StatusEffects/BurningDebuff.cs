using UnityEngine;

public class BurningDebuff : StatusEffect
{
    [SerializeField]
    protected int _damagePerTick;

    [SerializeField]
    protected float _tickInterval;

    [SerializeField]
    private GameObject _fireParticleSystemPrefab;

    private ParticleSystem _fireParticleInstance;

    private float _timePassed;
    protected bool _movingToPoint;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnApply(IBuffable owner, GameObject ownerGameObject, SpriteRenderer ownerRenderer, 
        Navigation navigation = null)
    {
        base.OnApply(owner, ownerGameObject, ownerRenderer, navigation);

        _fireParticleInstance = Instantiate(_fireParticleSystemPrefab, ownerGameObject.transform).GetComponent<ParticleSystem>();

        ParticleSystem.ShapeModule shape = _fireParticleInstance.shape;
        shape.spriteRenderer = ownerRenderer;

        _timePassed = _tickInterval;
        _movingToPoint = false;
    }

    public override void Navigation()
    {
        if (_navigation != null)
        {
            if (!_movingToPoint)
            {
                _navigation.MoveTo(_ownerGameObject.transform.position.ToVector2() 
                    + Utility.RandomPointOnCircleEdge(5.0f * Random.Range(1.0f, 2.0f)), true);
                _movingToPoint = true;                                                                           
            }
            else if(_navigation.AtDestination())
            {
                _movingToPoint = false;
            }
        }

        base.Navigation();
    }

    private void Update()
    {
        _timePassed += Time.deltaTime;
        if (_timePassed >= _tickInterval)
        {
            _timePassed = 0.0f;

            int damagePerTick = _damagePerTick;
            if (_owner is Player)
            {
                damagePerTick /= 10;
            }

            if (_owner.ReceiveDamage(damagePerTick, Vector2.zero, false, false))
            {
                BeforeDestroyed();
            }
        }
    }

    protected override void BeforeDestroyed()
    {
        var main = _fireParticleInstance.main;
        main.loop = false;
        Destroy(_fireParticleInstance, _fireParticleInstance.main.duration);
        base.BeforeDestroyed();
    }
}
