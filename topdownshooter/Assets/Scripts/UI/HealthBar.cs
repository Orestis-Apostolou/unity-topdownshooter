using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public HealthSystem healthSystem;
    private Slider slider;
    public Image fillImage;
    public Gradient gradient;
    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        float percent = healthSystem.HealthPercent();
        slider.value = percent;

        fillImage.color = gradient.Evaluate(percent);
    }
}
