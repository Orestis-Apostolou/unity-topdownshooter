using System.Collections.Specialized;
using UnityEngine;

public abstract class Attack: MonoBehaviour
{
    [Header("Attack Properties")]
    public float speed;
    public float damage;
    public float lifetime;
    public GameObject owner;

    public virtual void Initialize() { }
    protected virtual void Update() {}
    public virtual bool IsInRange(Vector3 ownerPos, Vector3 point, float threshold = 1f)
    {
        threshold = Mathf.Clamp01(threshold);
        return Vector3.Distance(ownerPos, point) <= speed * lifetime * threshold;
    }
}
