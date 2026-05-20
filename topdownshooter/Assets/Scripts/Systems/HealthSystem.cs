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

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}   
