using LootLocker.Requests;
using UnityEngine;

public class LeaderboardService : MonoBehaviour
{
    public static int leaderboardID = 32066;

    /// <summary>
    /// Suma score al total del usuario en el leaderboard.
    /// </summary>
    public static void AddScore(int addedScore, string leaderboardKey, System.Action<bool> onDone = null)
    {
        string playerID = PlayerPrefs.GetString("PlayerID", "");
        if (string.IsNullOrEmpty(playerID))
        {
            Debug.LogError("PlayerID no encontrado, no se puede enviar score.");
            return;
        }

        // Primero obtener el score actual del jugador
        LootLockerSDKManager.GetMemberRank(leaderboardKey, playerID, (response) =>
        {
            if (!response.success)
            {
                Debug.LogError("Error al obtener score actual.");
                return;
            }

            int currentScore = response.score;
            int newScore = currentScore + addedScore;

            Debug.Log($"Score actual: {currentScore}, agregar: {addedScore}, total nuevo: {newScore}");

            // Subir el nuevo total
            LootLockerSDKManager.SubmitScore(playerID, newScore, leaderboardKey, (submitResponse) =>
            {
                if (!submitResponse.success)
                {
                    Debug.LogError("No se pudo subir el score acumulado.");
                    return;
                }

                Debug.Log("Score acumulado actualizado correctamente: " + newScore);
            });
        });
    }
}
