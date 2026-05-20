using UnityEngine;
using UnityEngine.UI;

public class HeatBar : MonoBehaviour
{
    public HeatSystem heatSystem;
    private Slider slider;
    public Image fillImage;
    public Gradient gradient;
    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        float percent = heatSystem.HeatPercent();
        slider.value = percent;

        fillImage.color = gradient.Evaluate(percent);
    }
}
