using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    [SerializeField]
    Slider slider;

    public void setMaxValue(int maxValue) {
        slider.maxValue = maxValue;
        slider.value = maxValue;
    }

    public void setValue(int value) {
        slider.value = value;
    }
}