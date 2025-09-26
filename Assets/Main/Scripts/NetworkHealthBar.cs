using UnityEngine;
using UnityEngine.UI;

public class NetworkHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    private HealthScript health;

    void Awake()
    {
        health = transform.root.GetComponent<HealthScript>();
    }

    void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(health._currentHealth, health.maxHealth);
        }
    }

    void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= UpdateHealthBar;
        }
    }

    void UpdateHealthBar(int current, int max)
    {
        healthBar.fillAmount = (float)current / max;
    }
}

