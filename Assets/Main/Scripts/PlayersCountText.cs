using UnityEngine;
using TMPro;

public class PlayersCountText : MonoBehaviour
{
    [SerializeField] private TMP_Text alivePlayersText;

    private void OnEnable()
    {
        GameManager.OnAlivePlayersChanged += UpdateAlivePlayers;
    }

    private void OnDisable()
    {
        GameManager.OnAlivePlayersChanged -= UpdateAlivePlayers;
    }

    private void UpdateAlivePlayers(int count)
    {
        alivePlayersText.text = $"Players Alive: {count}";
    }
}
