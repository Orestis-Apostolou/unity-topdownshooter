using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public CombatantHealth healthSystem;
    public Image fillImage;
    public Gradient gradient;

    void Update()
    {
        float percent = healthSystem.HealthPercent();
        fillImage.fillAmount = percent;
        fillImage.color = gradient.Evaluate(percent);
    }
}
