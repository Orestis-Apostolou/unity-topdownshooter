using UnityEngine;

public class FiringSystem : MonoBehaviour
{
    [Header("Firing Settings")]
    public GameObject attackPrefab;
    public float fireRate = 4f;
    public float spreadRange = 45f; // In degrees

    private float fireCooldown = 1.0f;
    private Attack AttackScript;

    private void Awake()
    {
        AttackScript = attackPrefab.GetComponent<Attack>();
        if (AttackScript == null)
            Debug.LogError("No 'Attack' script on prefab");
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;
    }

    public bool CanFire()
    {
        return fireCooldown <= 0f;
    }

    public bool IsInRange(Vector3 point, float threshold=1)
    {
        return AttackScript.IsInRange(gameObject.transform.position, point, threshold);
    }

    public void Fire()
    {
        if (CanFire())
        {
            GameObject attack_obj = Instantiate(attackPrefab, transform.position, transform.rotation);
            Attack attack = attack_obj.GetComponent<Attack>();
            attack.owner = gameObject;

            fireCooldown = 1.0f / fireRate;
        }
    }

    public void FireSpread()
    {
        if (CanFire())
        {
            float angle = Random.Range(-spreadRange, spreadRange);
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward) * transform.rotation;

            GameObject attack_obj = Instantiate(attackPrefab, transform.position, rot);
            Attack attack = attack_obj.GetComponent<Attack>();
            attack.owner = gameObject;

            fireCooldown = 1.0f / fireRate;
        }
    }

    public float getProjSpeed()
    {
        return AttackScript.speed;
    }
}
