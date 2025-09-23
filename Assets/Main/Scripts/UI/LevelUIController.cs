using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class LevelUIController : MonoBehaviour // maneja UI del nivel
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text zombieCountText;
    [SerializeField] private TMP_Text playersAliveText;
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
    }

    private void HandleWaveStarted(int wave, int amount, bool bossWave)
    {
        waveText.text = "Round: " + wave;
        zombieCountText.text = "Zombies: " + amount;
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

}
