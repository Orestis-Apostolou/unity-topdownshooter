using System.Threading;
using UnityEngine;

public class Bullet : Attack
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        base.Update();

        // Destroy after lifetime ends
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + (Vector2)transform.up * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore collision if colliding with attack owner
        if (other.gameObject != owner)
        {
            HealthSystem health = other.GetComponent<HealthSystem>();

            // If colliding with something with a HealthSystem deal damage
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            // Destroy bullet on collision with anything other than owner
            Destroy(gameObject);
        }
    }
}
