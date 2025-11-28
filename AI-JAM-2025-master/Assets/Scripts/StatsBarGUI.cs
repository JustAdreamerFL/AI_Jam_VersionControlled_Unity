using System;
using UnityEngine;
using UnityEngine.UI;

public class StatsBarGUI : MonoBehaviour
{
    private Image healthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar = transform.Find("ZoneBar/BarVisual").GetComponent<Image>();
    }

    public string Name => gameObject.name;

    internal void UpdateStatBar(float averageHealth, float min, float max) {
        healthBar.fillAmount = (Mathf.Clamp(averageHealth, min, max) - min) / (max - min);
        SetNonlinearColor(healthBar, 3f);
    }

    internal void UpdateStatBar(IHealth healthComponent) {
        if (healthComponent == null) {
            return;
        }
        healthBar.fillAmount = Mathf.Clamp(healthComponent.CurrentHealth, 0f, 1f);
        SetNonlinearColor(healthBar, 3f);
    }

    private void SetNonlinearColor(Image healthBar, float nonLinearity) {
        var exponentialFill = (Mathf.Pow(nonLinearity, healthBar.fillAmount) - 1f) / (nonLinearity - 1f);

        healthBar.color = Color.Lerp(Color.red, Color.white, exponentialFill);
    }
}
