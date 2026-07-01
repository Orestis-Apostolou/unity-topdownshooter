using UnityEngine;
using UnityEngine.UI;

public class HeatBar : MonoBehaviour
{
    public HeatSystem heatSystem;
    public Image fillImage;
    public Gradient gradient;
    void Update()
    {
        float percent = heatSystem.HeatPercent();
        fillImage.fillAmount = percent;
        fillImage.color = gradient.Evaluate(percent);
    }
}
