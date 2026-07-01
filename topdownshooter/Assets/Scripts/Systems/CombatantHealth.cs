using UnityEngine;

public class CombatantHealth : HealthSystem
{
    public override void TakeDamage(float damage)
    {
        // Safeguard so agent can't take damage during round reset
        if (GameManager.Instance.roundOver)
            return;

        health -= damage;
        GameManager.Instance.OnAgentDamaged(gameObject);

        if (health <= 0)
        {
            OnDeath();
        }
    }

    protected override void OnDeath()
    {
        GameManager.Instance.OnAgentDied(gameObject);
    }
}

