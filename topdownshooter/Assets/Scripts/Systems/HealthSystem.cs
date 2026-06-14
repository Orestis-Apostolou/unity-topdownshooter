using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    protected float health;
    public float maxHealth = 100f;

    private void Awake()
    {
        health = maxHealth;
    }

    //public void Update()
    //{
    //    Debug.Log("Health: " + HealthPercent() * 100 + "%");
    //}

    public float HealthPercent()
    {
        return health / maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        // Safeguard so bullets don't deal damage while the round gets reset
        if (GameManager.Instance.roundOver)
            return;
        
        health -= damage;
        GameManager.Instance.OnAgentDamaged(gameObject);
        
        if (health <= 0)
        {
            OnDeath();
        }
    }

    public void ResetHealth()
    {
        health = maxHealth;
    }

    protected virtual void OnDeath()
    {
        GameManager.Instance.OnAgentDied(gameObject);
    }
}   
