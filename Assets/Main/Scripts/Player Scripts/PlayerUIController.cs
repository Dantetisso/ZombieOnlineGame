using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour // maneja UI del jugador
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image evadeBar;
    [SerializeField] private TMP_Text ammoCountText;

    private HealthScript health;
    private PlayerMovement player;
    private Gun myGun;

    void Awake()
    {
        health = transform.root.GetComponent<HealthScript>();
        player = transform.root.GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(health._currentHealth, health.maxHealth);
        }

        if (player != null)
        {
            player.OnChangeGun += InitGun;
        }
    }

    void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= UpdateHealthBar;
        }

        if (myGun != null)
        {
            myGun.OnAmmoChange -= UpdateAmmoText;
        }

        if (player != null)
        {
            player.OnChangeGun -= InitGun;
        }
    }

    void Update()
    {
        if (player != null)
        {
            UpdateEvadeBar();
        }
    }

    // Inicializa o cambia el arma en la UI
    public void InitGun(Gun gun)
    {
        if (myGun != null)
        {
            myGun.OnAmmoChange -= UpdateAmmoText; // Remuevo listener del arma anterior
        }

        myGun = gun;

        if (myGun != null)
        {
            myGun.OnAmmoChange += UpdateAmmoText; // Agrego listener del arma actual
            UpdateAmmoText(myGun._currentAmmo, myGun._maxAmmo); // Actualizo UI inmediatamente
        }
    }

    void UpdateHealthBar(int current, int max)
    {
        healthBar.fillAmount = (float)current / max;
    }

    void UpdateEvadeBar()
    {
        evadeBar.fillAmount = player.GetevadeTime();
    }

    private void UpdateAmmoText(int clip, int reserve)
    {
        ammoCountText.text = $"{clip} / {reserve}";
    }
}
