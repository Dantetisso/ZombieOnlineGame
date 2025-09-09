using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Image bar;
    [SerializeField] private HealthScript health;

    void OnEnable()
    {
        if (health != null) // si cambia la vida actualizo la barra
        {
            health.OnHealthChanged += Refresh;
            Refresh(health.CurrentHealth, health.maxHealth);
        }
    }

    void OnDisable()
    {
        if (health != null)
            health.OnHealthChanged -= Refresh;
    }

    void Refresh(int current, int max)
    {
        bar.fillAmount = (float)current / max;
    }
}
