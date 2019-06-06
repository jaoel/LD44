using UnityEngine;

public interface IBuffable
{
    bool ReceiveDamage(int damage, Vector2 velocity, bool maxHealth = false, bool spawnBloodSpray = true);
    SpriteRenderer GetSpriteRenderer();
}
