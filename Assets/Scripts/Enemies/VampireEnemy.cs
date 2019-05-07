using UnityEngine;

public class VampireEnemy : Enemy {
    public GameObject vampireVisuals;
    public GameObject batVisuals;

    protected override void FixedUpdate() {
        base.FixedUpdate();
    }

    protected override bool PlayAttackAnimation() {
        return Vector2.Distance(_target, transform.position) < _stoppingDistance;
    }
} 
