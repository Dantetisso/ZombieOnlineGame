using UnityEngine;
using TMPro;

public class LevelUIController : MonoBehaviour // maneja UI del nivel
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text zombieCountText;
    [SerializeField] private TMP_Text playersAliveText;
    [SerializeField] private GameObject bossWarningObject;  
    [SerializeField] private GameManager gameManager;

    private void OnEnable()
    {
        GameManager.OnAlivePlayersChanged += UpdateAlivePlayers;
        GameManager.OnZombiesAliveChanged += UpdateAliveCount;
        GameManager.OnWaveStarted += HandleWaveStarted;
        GameManager.OnVictory += HandleVictory;
    }

    private void OnDisable()
    {
        GameManager.OnAlivePlayersChanged -= UpdateAlivePlayers;
        GameManager.OnZombiesAliveChanged -= UpdateAliveCount;
        GameManager.OnWaveStarted -= HandleWaveStarted;
        GameManager.OnVictory -= HandleVictory;
    }

    private void Start()
    {
        if (gameManager != null && gameManager.CurrentWave > 0)
        {
            waveText.text = "Round: " + gameManager.CurrentWave;
            zombieCountText.text = "Zombies: " + gameManager.ZombiesAlive;
        }

        // Asegurarnos de que el GameObject esté apagado al principio
        bossWarningObject.SetActive(false);
    }

    private void HandleWaveStarted(int wave, int amount, bool bossWave)
    {
        waveText.text = "Round: " + wave;
        zombieCountText.text = "Zombies: " + amount;

        // Ya no mostramos un mensaje de texto, solo activamos el GameObject cuando es ronda de boss
        SetBossWarningActive(bossWave);
    }

    private void HandleVictory()
    {
        waveText.text = "Victory!";
        zombieCountText.text = "0";
    }

    public void UpdateAliveCount(int alive)
    {
        zombieCountText.text = "Zombies: " + alive;
    }

    private void UpdateAlivePlayers(int count)
    {
        playersAliveText.text = "Players Alive: " + count;
    }

    // Nuevo método para activar/desactivar el GameObject de advertencia del boss
    public void SetBossWarningActive(bool isActive)
    {
        bossWarningObject.SetActive(isActive);  // Activa o desactiva el GameObject
    }
}
