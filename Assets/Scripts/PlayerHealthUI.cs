using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth; // assign your Player (with PlayerHealth)
    [SerializeField] private Slider slider;             // optional; will auto-grab from this GO

    private int lastValue = -1;

    private void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();

        int max = playerHealth ? playerHealth.MaxHealth : 100;

        slider.minValue = 0;
        slider.maxValue = max;
        slider.wholeNumbers = true;

        SetValue(playerHealth ? playerHealth.CurrentHealth : max);
    }

    private void Update()
    {
        if (!playerHealth) return;

        int current = playerHealth.CurrentHealth;
        if (current != lastValue)
            SetValue(current);
    }

    private void SetValue(int v)
    {
        lastValue = v;
        slider.value = v;
    }
}
