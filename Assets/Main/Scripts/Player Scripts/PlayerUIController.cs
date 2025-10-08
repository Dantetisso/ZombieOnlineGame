using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour // maneja UI del jugador
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image evadeBar;
    [SerializeField] private Image ammoBar;
    [SerializeField] private Image gunImage;
    [SerializeField] private TMP_Text ammoClipText;
    [SerializeField] private TMP_Text ammoReserveText;

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
            UpdateAmmoText(myGun.CurrentAmmo, myGun.MaxAmmo); // Actualizo UI inmediatamente
            myGun.SetUIImage(gunImage);
        }
        else
        {
            // Si no hay arma activa, limpia el texto
            ammoClipText.text = "0";
            ammoReserveText.text = "0";
        }
    }

    void UpdateHealthBar(int current, int max)
    {
        healthBar.fillAmount = (float)current / max;
    }

    void UpdateEvadeBar()
    {
        evadeBar.fillAmount = player.GetEvadeTime();
    }

    private void UpdateAmmoText(int clip, int reserve)
    {
        ammoClipText.text = clip.ToString();
        ammoReserveText.text = reserve.ToString();

        ammoBar.fillAmount = (float)clip / myGun.fullAmmoClip;
    }
}
