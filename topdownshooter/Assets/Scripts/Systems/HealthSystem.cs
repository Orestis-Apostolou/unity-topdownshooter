using Unity.VisualScripting;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    protected float health;
    public float maxHealth = 100f;

    protected void Awake()
    {
        health = maxHealth;
    }

    public float HealthPercent()
    {
        return health / maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        // Safeguard so object doesn't get damaged between rounds
        if (GameManager.Instance.roundOver)
            return;
        
        health -= damage;
        
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
        Destroy(gameObject);
    }
}   
